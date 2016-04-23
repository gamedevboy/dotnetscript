using System.Collections.Generic;
using DotNetScript.Runtime;
using DotNetScript.Types.Reference;

namespace DotNetScript.Types
{
    internal struct ScriptCallArguments
    {
        private readonly object[] _arguments;
        public object[] Arguments => _arguments; 

        public ScriptCallArguments(ScriptMethodBase scriptMethod) 
            : this(scriptMethod.MethodDefinition.Parameters.Count, scriptMethod.ParamTypes)
        {
            
        }

        public ScriptCallArguments(int argCount, IReadOnlyList<ScriptType> paramTypes)
        {
            _arguments = new object[argCount];

            for (var i = _arguments.Length - 1; i >= 0; i--)
            {
                var arg = RuntimeContext.Current.PopFromStack();
                var @ref = arg as IScriptReference;
                if (@ref != null)
                    arg = @ref.Value;

                var scriptObjectArg = arg as ScriptObject;

                if (scriptObjectArg != null && paramTypes[i].IsHost)
                    arg = scriptObjectArg.HostInstance;

                if (scriptObjectArg == null && !paramTypes[i].IsHost)
                    arg = ScriptObject.FromHostObject(arg);

                _arguments[i] = arg;
            }
        }
    }
}
