using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DotNetScript.Types;
using DotNetScript.Types.Attributes;

namespace DotNetScript.Runtime
{
    [ScriptHostWarpper(typeof(IAsyncStateMachine))]
    public class ScriptAsyncStateMachine : IAsyncStateMachine
    {
        private ScriptObject _scriptObject;
        private ScriptObject ScriptObject => _scriptObject ?? (_scriptObject = ScriptObject.FromHostObject(this));

        public void MoveNext()
        {
            ScriptObject.Invoke("MoveNext");
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            ScriptObject.Invoke("SetStateMachine", stateMachine);
        }
    }
}
