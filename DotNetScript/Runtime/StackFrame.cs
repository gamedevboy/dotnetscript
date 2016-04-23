using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotNetScript.Types;

namespace DotNetScript.Runtime
{
    public class StackFrame
    {
        [ThreadStatic]
        private static Stack<StackFrame> _freeList;
        private static Stack<StackFrame> FreeList => _freeList ?? (_freeList = new Stack<StackFrame>());
        private readonly Stack<object> _objectStack = new Stack<object>();

        private object[] _arguments;
        internal object[] Arguments => _arguments;

        private object[] _locals;
        internal object[] Locals => _locals; 

        internal ScriptMethodBase ScriptMethod { get; private set; }

        internal static StackFrame Alloc(ScriptMethodBase scriptMethod, params object[] param)
        {
            return FreeList.Count == 0 ? new StackFrame().Init(scriptMethod, param) : FreeList.Pop().Init(scriptMethod, param);
        }

        private StackFrame Init(ScriptMethodBase scriptMethod, params object[] param)
        {
            // init local vars
            _locals = scriptMethod.MethodDefinition.Body.Variables.Select(_ =>
            {
                var type = _.VariableType;
                if (!type.IsValueType) return null;

                var scriptType = ScriptContext.Get(type.Module).TypeSystem.GetType(type);
                return scriptType.IsHost ? Activator.CreateInstance(scriptType.HostType) : scriptType.CreateInstance();
            }).ToArray();

            // init arguments
            _arguments = param;

            ScriptMethod = scriptMethod;

            return this;
        }

        internal static void Free(StackFrame frame)
        {
            FreeList.Push(frame);
        }

        internal void Push(object value)
        {
            _objectStack.Push(value);
        }

        internal object Pop()
        {
            return _objectStack.Count > 0 ? _objectStack.Pop() : null;
        }

        internal object Return()
        {
            return Pop();
        }

        internal object Peek()
        {
            Debug.Assert( _objectStack.Count > 0 );

            return _objectStack.Peek();
        }
    }
}
