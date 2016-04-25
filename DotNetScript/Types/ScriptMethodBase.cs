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

            if (IsHost || DeclareType.IsDelegate)
            {
                if (scriptObject?.HostInstance != null)
                    target = scriptObject.HostInstance;

                return GetNativeMethod(args.Select(_ => _?.GetType()).ToArray())?.Invoke(target, args);
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
