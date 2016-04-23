using Mono.Cecil;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DotNetScript.Types
{
    public class ScriptConstructorInfo : ScriptMethodBase
    {
        private static readonly MethodInfo[] NativeConstructor = typeof(ScriptObject).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(_ => _.Name == "NativeConstruct").ToArray();

        internal ScriptConstructorInfo(ScriptType declareType, MethodDefinition methodDef)
            : base(declareType, methodDef)
        {
            
        }

        protected override MethodBase GetNativeMethod(Type[] types)
        {
            Debug.Assert(DeclareType.HostType != null);
            return NativeConstructor[types.Length];
        }
    }
}
