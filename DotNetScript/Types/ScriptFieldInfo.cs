using System.Reflection;
using Mono.Cecil;

namespace DotNetScript.Types
{
    public class ScriptFieldInfo : ScriptMemberInfo
    {
        private readonly FieldDefinition _fieldDef;

        internal ScriptFieldInfo(ScriptType declareType, FieldDefinition fieldDef)
            : base(declareType, fieldDef)
        {
            _fieldDef = fieldDef;
        }

        private FieldInfo GetNativeFieldInfo()
        {
            return DeclareType.HostType.GetField(_fieldDef.Name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        }

        public void SetValue(object target, object value)
        {
            var scriptObject = target as ScriptObject;

            if (IsHost || scriptObject == null)
                GetNativeFieldInfo().SetValue(target, value);
            else
                scriptObject[_fieldDef] = value;
        }

        public object GetValue(object target)
        {
            var scriptObject = target as ScriptObject;

            if (IsHost || scriptObject == null)
                return GetNativeFieldInfo().GetValue(target);

            return scriptObject[_fieldDef];
        }
    }
}
