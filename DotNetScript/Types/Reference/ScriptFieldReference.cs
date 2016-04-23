using DotNetScript.Runtime;
using Mono.Cecil;

namespace DotNetScript.Types.Reference
{
    internal struct ScriptFieldReference : IScriptReference
    {
        private readonly ScriptFieldInfo _scriptField;
        private readonly object _target;

        public ScriptFieldReference(object target, FieldReference fieldRef)
        {
            _target = target;
            _scriptField = ScriptContext.Get(fieldRef.Module)?.TypeSystem.GetType(fieldRef.DeclaringType).GetField(fieldRef);
        }

        public object Value
        {
            get { return _scriptField.GetValue(_target); }
            set { _scriptField.SetValue(_target, value); }
        }
    }
}