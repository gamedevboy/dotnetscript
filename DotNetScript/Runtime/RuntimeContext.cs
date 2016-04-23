using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotNetScript.Types;
//using StackFrame = DotNetScript.Runtime.StackFrame;

namespace DotNetScript.Runtime
{
    public class RuntimeContext
    {
        private const int MaxStackFrameCount = 32;

        [ThreadStatic]
        private static RuntimeContext _current;
        public static RuntimeContext Current => _current ?? (_current = new RuntimeContext());

        private readonly ScriptInterpreter _scriptInterpreter;
        internal ScriptInterpreter Interpreter => _scriptInterpreter;

        private readonly Stack<StackFrame> _stackFrames = new Stack<StackFrame>(MaxStackFrameCount);
        internal StackFrame CurrentStackFrame => _stackFrames.Peek();

        private RuntimeContext()
        {
            _scriptInterpreter = new ScriptInterpreter(this);
        }

        internal void PushCallStack(ScriptMethodBase scriptMethod, params object[] param)
        {
            _stackFrames.Push(StackFrame.Alloc(scriptMethod, param));
        }

        internal StackFrame PopCallStack()
        {
            return _stackFrames.Pop();
        }

        internal void PushToStack(object value)
        {
            Debug.Assert(CurrentStackFrame != null);

            CurrentStackFrame.Push(value);
        }

        internal object PopFromStack()
        {
            Debug.Assert(CurrentStackFrame != null);

            return CurrentStackFrame.Pop();
        }

        internal object PeekFromStack()
        {
            Debug.Assert(CurrentStackFrame != null);

            return CurrentStackFrame.Peek();
        }

        public bool IsMethodOnStack(ScriptMethodInfo method)
        {
            return _stackFrames.Any(_ => _.ScriptMethod == method);
        }
    }
}
