using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotNetScript.Runtime;
using Mono.Cecil;

namespace DotNetScript.Types
{
    public class ScriptType : ScriptMemberInfo
    {
        private readonly TypeDefinition _typeDef;

        public TypeDefinition TypeDefinition => _typeDef;
        public ScriptAssembly ScriptAssembly { get; }
        public virtual Type HostType { get; }
        public virtual Type WarpperedType { get; }

        public bool IsClass => _typeDef.IsClass;
        public bool IsValueType => _typeDef.IsValueType;
        public bool IsEnum => _typeDef.IsEnum;
        public bool IsPublic => _typeDef.IsPublic;
        public bool IsNesedPublic => _typeDef.IsNestedPublic;
        public bool IsInterface => _typeDef.IsInterface;
        public bool IsNesed => _typeDef.IsNested;
        public bool IsDelegate { get; }

        private List<ScriptMethodInfo> _methods;
        private List<ScriptConstructorInfo> _constructors;

        private static readonly HashSet<string> StaticConstructor = new HashSet<string>();

        private ConcurrentDictionary<FieldDefinition, ScriptFieldInfo> _fieldInfos;

        public IReadOnlyList<ScriptMethodInfo> Methods => _methods;
        public IReadOnlyList<ScriptConstructorInfo> Constructors => _constructors;

        private readonly List<ScriptType> _genericTypes = new List<ScriptType>();
        public virtual IReadOnlyList<ScriptType> GenericTypes => _genericTypes;

        private readonly ScriptType _baseType;
        public ScriptType BaseType => _baseType;

        private bool _isInitialized;

        internal ScriptType(TypeDefinition typeDef, ScriptAssembly scriptAssembly, params ScriptType[] genericTypes) :
            this(typeDef, scriptAssembly)
        {
            _baseType = _typeDef.Interfaces.Any(_ => _.Name == "IAsyncStateMachine") ? ScriptContext.GetType(typeof(ScriptAsyncStateMachine)) : ScriptContext.GetType(_typeDef.BaseType);

            HostType = GetHostType();
            IsDelegate = HostType.BaseType?.Name == "MulticastDelegate";
            _genericTypes.AddRange(genericTypes);

            Initialize();
        }

        internal ScriptType Initialize()
        {
            if (_isInitialized)
                return this;

            _isInitialized = true;

            var methods = _typeDef.Methods;
            _methods = methods.Where(_ => !_.IsConstructor).Select(_ => new ScriptMethodInfo(this, _)).ToList();
            _constructors = methods.Where(_ => _.IsConstructor).Select(_ => new ScriptConstructorInfo(this, _)).ToList();

            _fieldInfos =
                new ConcurrentDictionary<FieldDefinition, ScriptFieldInfo>(_typeDef.Fields.ToDictionary(_ => _,
                    _ => new ScriptFieldInfo(this, _)));

            if (IsHost) return this;

            lock (StaticConstructor)
            {
                if (StaticConstructor.Contains(_typeDef.FullName)) return this;

                StaticConstructor.Add(_typeDef.FullName);
                _constructors.FirstOrDefault(_ => _.Name == ".cctor")?.Invoke(null);
            }

            return this;
        }

        protected ScriptType(TypeDefinition typeDef, ScriptAssembly scriptAssembly) :
            base(scriptAssembly.TypeSystem.GetType(typeDef.DeclaringType), typeDef)
        {
            _typeDef = typeDef;
            ScriptAssembly = scriptAssembly;
        }

        internal ScriptFieldInfo GetField(FieldReference fieldRef)
        {
            return _fieldInfos[fieldRef.Resolve()];
        }

        internal ScriptMethodBase GetMethod(MethodReference methodRef, ScriptMethodInfo ownerMethod = null)
        {
            var methodDef = methodRef.Resolve();
            var genericMethod = methodRef as GenericInstanceMethod;

            var ret = _methods.FirstOrDefault(_ => _.MethodDefinition == methodDef);

            if (ret == null) return _constructors.FirstOrDefault(_ => _.MethodDefinition == methodDef);

            if (genericMethod != null && genericMethod.HasGenericArguments)
                ret = ret.MakeGeneric(genericMethod.GenericArguments.Select(_ =>
                {
                    if (_.IsGenericParameter)
                    {
                        var genericParameter = (GenericParameter)_;
                        switch (genericParameter.Type)
                        {
                            case GenericParameterType.Type:
                                {
                                    var ownerTypeDef = (TypeDefinition)genericParameter.Owner;
                                    Debug.Assert(ownerTypeDef == _typeDef);
                                    return _genericTypes[genericParameter.Position];
                                }
                            case GenericParameterType.Method:
                                {
                                    var ownerMethodDef = (MethodDefinition)genericParameter.Owner;

                                    Debug.Assert(ownerMethod != null);
                                    Debug.Assert(ownerMethodDef == ownerMethod.MethodDefinition);

                                    return ownerMethod.GenericTypes[genericParameter.Position];
                                }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                    {
                        return ScriptContext.GetType(_);
                    }
                }).ToArray());

            return ret;
        }

        public ScriptMethodInfo GetMethod(string name)
        {
            return _methods.FirstOrDefault(_ => _.MethodDefinition.Name == name);
        }

        public ScriptMethodInfo GetMethod(string name, ScriptType[] types)
        {
            return _methods.FirstOrDefault(_ => _.MethodDefinition.Name == name);
        }

        public ScriptObject CreateInstance(params object[] param)
        {
            return ScriptAssembly.TypeSystem.CreateInstance(this, param);
        }

        public virtual ScriptType MakeGeneric(params ScriptType[] genericTypes)
        {
            return new ScriptType(_typeDef, ScriptAssembly, genericTypes);
        }

        public ScriptConstructorInfo GetConsturctor(params ScriptType[] types)
        {
            return _constructors.FirstOrDefault(_ => _.MethodDefinition.Parameters.Count == types.Length);
        }

        protected virtual Type GetHostType()
        {
            if (BaseType?.Name == "MulticastDelegate")
            {
                var invokeMethodDef = TypeDefinition.Methods.FirstOrDefault(_ => _.Name == "Invoke");
                if (invokeMethodDef != null)
                {
                    return ScriptDelegate.GetDelegateType(invokeMethodDef.Parameters.Count);
                }
            }

            return BaseType?.HostType;
        }

        public bool IsInstanceOfType(object o)
        {
            var scriptObject = o as ScriptObject;

            if (IsHost)
            {
                return HostType.IsInstanceOfType(scriptObject ?? o);
            }

            return scriptObject?.GetScriptType().IsAssignFrom(this) ?? HostType.IsInstanceOfType(o);
        }

        public bool IsAssignFrom(ScriptType scriptType)
        {
            if (scriptType == null)
                return false;

            return scriptType == this || BaseType.IsAssignFrom(scriptType);
        }
    }
}
