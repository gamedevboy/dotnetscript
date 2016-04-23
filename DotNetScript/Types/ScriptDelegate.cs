using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetScript.Types
{
    internal class ScriptDelegate
    {
        private readonly ScriptObject _targetObject;
        private readonly ScriptMethodBase _method;
        private static readonly IntPtr[] InvokePtr = new IntPtr[5];
        private static readonly Type[] DelegateTypes = new Type[5];

        static ScriptDelegate()
        {
            InvokePtr[0] = typeof(ScriptDelegate).GetMethod("ScriptInvoke0", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).MethodHandle.GetFunctionPointer();
            InvokePtr[1] = typeof(ScriptDelegate).GetMethod("ScriptInvoke1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).MethodHandle.GetFunctionPointer();
            InvokePtr[2] = typeof(ScriptDelegate).GetMethod("ScriptInvoke2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).MethodHandle.GetFunctionPointer();
            InvokePtr[3] = typeof(ScriptDelegate).GetMethod("ScriptInvoke3", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).MethodHandle.GetFunctionPointer();
            InvokePtr[4] = typeof(ScriptDelegate).GetMethod("ScriptInvoke4", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).MethodHandle.GetFunctionPointer();

            DelegateTypes[0] = typeof(ScriptInvokeDelegate0);
            DelegateTypes[1] = typeof(ScriptInvokeDelegate1);
            DelegateTypes[2] = typeof(ScriptInvokeDelegate2);
            DelegateTypes[3] = typeof(ScriptInvokeDelegate3);
            DelegateTypes[4] = typeof(ScriptInvokeDelegate4);
        }

        private delegate object ScriptInvokeDelegate0();
        private delegate object ScriptInvokeDelegate1(object arg1);
        private delegate object ScriptInvokeDelegate2(object arg1, object arg2);
        private delegate object ScriptInvokeDelegate3(object arg1, object arg2, object arg3);
        private delegate object ScriptInvokeDelegate4(object arg1, object arg2, object arg3, object arg4);

        public ScriptDelegate(object targetObject, ScriptMethodBase method)
        {
            _targetObject = ScriptObject.FromHostObject(targetObject);
            _method = method;
        }

        public static IntPtr GetInvokePtr(int index)
        {
            return InvokePtr[index];
        }

        public static Type GetDelegateType(int index)
        {
            return DelegateTypes[index];
        }

        private object ScriptInvoke0()
        {
            return _method.Invoke(_targetObject);
        }

        private object ScriptInvoke1(object arg1)
        {
            return _method.Invoke(_targetObject, arg1);
        }

        private object ScriptInvoke2(object arg1, object arg2)
        {
            return _method.Invoke(_targetObject, arg1, arg2);
        }

        private object ScriptInvoke3( object arg1, object arg2, object arg3)
        {
            return _method.Invoke(_targetObject, arg1, arg2, arg3);
        }

        private object ScriptInvoke4( object arg1, object arg2, object arg3, object arg4)
        {
            return _method.Invoke(_targetObject, arg1, arg2, arg3, arg4);
        }
    }
}
