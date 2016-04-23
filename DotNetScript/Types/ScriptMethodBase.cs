using System;
using System.Linq;
using DotNetScript.Runtime;
using Mono.Cecil;
using System.Reflection;

namespace DotNetScript.Types
{
    public abstract class ScriptMethodBase : ScriptMemberInfo
    {
        private readonly MethodDefinition _methodDef;
        internal MethodDefinition MethodDefinition => _methodDef;
        private readonly ScriptType[] _paramTypes;
        public ScriptType[] ParamTypes => _paramTypes;

        public bool HasThis => _methodDef.HasThis;
        public ScriptType ReturnType { get; }

        internal ScriptMethodBase(ScriptType declareType, MethodDefinition methodDef)
            : base(declareType, methodDef)
        {
            _methodDef = methodDef;

            if (!_methodDef.Parameters.Any(_=>_.ParameterType.IsGenericParameter || _.ParameterType.ContainsGenericParameter))
            {
                _paramTypes = _methodDef.Parameters.Select(_ =>
                {
                    if (_.ParameterType.Resolve() == declareType.TypeDefinition)
                        return declareType;

                    return ScriptContext.GetType(_.ParameterType);
                }).ToArray();
            }

            //ReturnType = ScriptContext.GetType(methodDef.ReturnType);
        }

        protected abstract MethodBase GetNativeMethod(Type[] types);

        public object Invoke(object target, params object[] args)
        {
            var scriptObject = target as ScriptObject;

            if (IsHost || DeclareType.IsDelegate)
            {
                if (scriptObject?.HostInstance != null)
                    target = scriptObject.HostInstance;

                return GetNativeMethod(args.Select(_ => _.GetType()).ToArray())?.Invoke(target, args);
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
