using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Mono.Cecil;

namespace DotNetScript.Types
{
    public class ScriptFieldInfo : ScriptMemberInfo
    {
        private readonly FieldDefinition _fieldDef;
        public bool IsStatic => _fieldDef.IsStatic;

        private ConcurrentDictionary<FieldDefinition, object> _staticFieldInfos = new ConcurrentDictionary<FieldDefinition, object>();

        internal ScriptFieldInfo(ScriptType declareType, FieldDefinition fieldDef)
            : base(declareType, fieldDef)
        {
            _fieldDef = fieldDef;
        }

        private FieldInfo GetNativeFieldInfo()
        {
            var ret = DeclareType.HostType.GetField(_fieldDef.Name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

            Debug.Assert(ret != null);

            return ret;
        }

        public void SetValue(object target, object value)
        {
            var scriptObject = target as ScriptObject;

            if (IsStatic && !IsHost)
                _staticFieldInfos[_fieldDef] = value;
            else
            {

                if (IsHost || scriptObject == null)
                    GetNativeFieldInfo().SetValue(target, value);
                else
                    scriptObject[_fieldDef] = value;
            }
        }

        public object GetValue(object target)
        {
            var scriptObject = target as ScriptObject;

            if (IsStatic && !IsHost)
            {
                return _staticFieldInfos.ContainsKey(_fieldDef) ? _staticFieldInfos[_fieldDef] : null;
            }

            if (IsHost || scriptObject == null)
            {
                return GetNativeFieldInfo().GetValue(target);
            }

            return scriptObject[_fieldDef];
        }
    }
}
