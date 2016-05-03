using System;
using System.Linq;
using DotNetScript.Runtime;
using Mono.Cecil;
using System.Reflection;
using System.Runtime.InteropServices;
using DotNetScript.Types.Reference;

namespace DotNetScript.Types
{
    public abstract class ScriptMethodBase : ScriptMemberInfo
    {
        private readonly MethodDefinition _methodDef;
        internal MethodDefinition MethodDefinition => _methodDef;
        public ScriptType[] ParamTypes { get; }

        public bool HasThis => _methodDef.HasThis;

        public ScriptType ReturnType { get; }

        public bool HasReturn { get; }

        internal ScriptMethodBase(ScriptType declareType, MethodDefinition methodDef)
            : base(declareType, methodDef)
        {
            _methodDef = methodDef;
            ReturnType = _methodDef.ReturnType.ContainsGenericParameter ? null : ScriptContext.GetType(_methodDef.ReturnType);
            HasReturn = ReturnType?.Name != "Void";

            if (!_methodDef.Parameters.Any(_=>_.ParameterType.IsGenericParameter || _.ParameterType.ContainsGenericParameter))
            {
                ParamTypes = _methodDef.Parameters.Select(_ => _.ParameterType.Resolve() == declareType.TypeDefinition ? declareType : ScriptContext.GetType(_.ParameterType)).ToArray();
            }
        }

        protected abstract MethodBase GetNativeMethod(Type[] types);

        public object Invoke(object target, params object[] args)
        {
            var scriptObject = target as ScriptObject;
            var scriptRef = target as IScriptReference;

            if (IsHost || DeclareType.IsDelegate)
            {
                if (scriptObject?.HostInstance != null)
                    target = scriptObject.HostInstance;

                if (scriptRef != null)
                    target = scriptRef.Value;

                for (var i = 0; i < args.Length; i++)
                {
                    scriptObject = args[i] as ScriptObject;

                    if (scriptObject?.HostInstance != null)
                        args[i] = scriptObject.HostInstance;
                }

                try
                {
                    return GetNativeMethod(args.Select(_ => _?.GetType()).ToArray())?.Invoke(target, args);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                if (scriptObject == null)
                    target = ScriptObject.FromHostObject(target);

                return RuntimeContext.Current.Interpreter.Invoke(this, target, args);
            }
        }
    }
}
