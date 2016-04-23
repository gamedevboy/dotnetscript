using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetScript.Types
{
    public class ScriptMethodInfo : ScriptMethodBase
    {
        private readonly List<ScriptType> _genericTypes = new List<ScriptType>();
        public IReadOnlyList<ScriptType> GenericTypes => _genericTypes;

        private readonly MethodInfo[] _nativeMethods;

        internal ScriptMethodInfo(ScriptType declareType, MethodDefinition methodDef, params ScriptType[] genericTypes)
            : base(declareType, methodDef)
        {
            _genericTypes.AddRange(genericTypes);
            _nativeMethods = ( DeclareType.HostType).GetMethods(BindingFlags.FlattenHierarchy| BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        }

        public ScriptMethodInfo MakeGeneric(params ScriptType[] genericTypes)
        {
            return new ScriptMethodInfo(DeclareType, MethodDefinition, genericTypes);
        }

        private bool IsTypesMatch(IReadOnlyCollection<Type> src, IReadOnlyList<Type> dest)
        {
            if (src.Count != dest.Count)
                return false;

            return !src.Where((t, i) => !t.IsAssignableFrom(dest[i])).Any();
        }

        protected override MethodBase GetNativeMethod(Type[] types)
        {
            var nativeMethod = _nativeMethods.FirstOrDefault(_ => _.Name == MethodDefinition.Name && IsTypesMatch(_.GetParameters().Select(p=>p.ParameterType).ToArray(), types));

            if (nativeMethod.ContainsGenericParameters)
            {
                nativeMethod = nativeMethod.MakeGenericMethod(_genericTypes.Select(_=>_.HostType).ToArray());
            }

            return nativeMethod;
        }
    }
}
