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
        private readonly Dictionary<int, MethodInfo> _nativeMethods = new Dictionary<int, MethodInfo>();

        internal ScriptMethodInfo(ScriptType declareType, MethodDefinition methodDef, params ScriptType[] genericTypes)
            : base(declareType, methodDef)
        {
            _genericTypes.AddRange(genericTypes);
            var methods = DeclareType.HostType.GetMethods(BindingFlags.FlattenHierarchy | 
                                                          BindingFlags.Public |
                                                          BindingFlags.NonPublic | 
                                                          BindingFlags.Static |
                                                          BindingFlags.Instance);
            foreach (var method in methods)
            {
                _nativeMethods[method.MetadataToken] = method;
            }
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
            MethodInfo nativeMethod;
            if (!_nativeMethods.TryGetValue(MethodDefinition.MetadataToken.ToInt32(), out nativeMethod))
            {
                if (DeclareType.IsDelegate)
                    nativeMethod = ScriptDelegate.GetDelegateType(types.Length).GetMethod("Invoke");
            }

            if (nativeMethod.ContainsGenericParameters)
            {
                nativeMethod = nativeMethod.MakeGenericMethod(_genericTypes.Select(_ => _.HostType).ToArray());
            }

            return nativeMethod;
        }
    }
}
