using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DotNetScript.Runtime;
using Mono.Cecil;

namespace DotNetScript.Types
{
    public class ScriptObject
    {
        private readonly ScriptType _scriptType;
        private readonly ConcurrentDictionary<string, object> _fields = new ConcurrentDictionary<string, object>();
        private static readonly ConditionalWeakTable<object, ScriptObject> ExternInfos = new ConditionalWeakTable<object, ScriptObject>();

        private object _hostInstance;

        public object HostInstance
        {
            get
            {
                return _hostInstance;
            }

            private set
            {
                _hostInstance = value;
                ExternInfos.Add(_hostInstance, this);
            }
        }

        internal ScriptObject(ScriptType scriptType)
        {
            Debug.Assert(_scriptType == null);
            _scriptType = scriptType;
        }

        public ScriptType GetScriptType()
        {
            Debug.Assert(_scriptType != null);

            return _scriptType;
        }

        #region NativeConstruct
        private void NativeConstruct() => HostInstance = Activator.CreateInstance(_scriptType.HostType);
        private void NativeConstruct(object arg0) => HostInstance = Activator.CreateInstance(_scriptType.HostType, arg0);
        private void NativeConstruct(object arg0, object arg1) => HostInstance = Activator.CreateInstance(_scriptType.HostType, arg0, arg1);
        private void NativeConstruct(object arg0, object arg1, object arg2) => HostInstance = Activator.CreateInstance(_scriptType.HostType, arg0, arg1, arg2);
        private void NativeConstruct(object arg0, object arg1, object arg2, object arg3) => HostInstance = Activator.CreateInstance(_scriptType.HostType, arg0, arg1, arg2, arg3);
        #endregion

        public THostType GetHostInstance<THostType>()
        {
            return (THostType)HostInstance;
        }

        public static ScriptObject FromHostObject(object obj)
        {
            if (obj == null) return null;

            ScriptObject ret;
            ExternInfos.TryGetValue(obj, out ret);
            return ret;
        }

        public Tuple<bool, object> Invoke(string methodName, params object[] args)
        {
            if( string.IsNullOrEmpty(methodName) )
                throw new ArgumentException("methodName is required !");

            var method = GetScriptType().GetMethod(methodName);

            return RuntimeContext.Current.IsMethodOnStack(method) ? Tuple.Create(false, (object)null) : Tuple.Create(method != null, method?.Invoke(method.IsHost ? this : HostInstance, args));
        }

        internal object this[FieldDefinition fieldDef]
        {
            get
            {
                object value;
                _fields.TryGetValue(fieldDef.Name, out value);
                return value;
            }
            set { _fields[fieldDef.Name] = value; }
        }
    }
}
