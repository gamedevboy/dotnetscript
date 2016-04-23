using DotNetScript.Runtime;
using Mono.Cecil;

namespace DotNetScript.Types
{
    public class ScriptMemberInfo
    {
        private readonly MemberReference _memberRef;

        public ScriptType DeclareType { get; }

        public string Name => _memberRef.Name;
        public bool IsHost => ScriptContext.IsHost(_memberRef.Module);

        internal ScriptMemberInfo(ScriptType declareType, MemberReference memberRef)
        {
            DeclareType = declareType;
            _memberRef = memberRef;
        }
    }
}
