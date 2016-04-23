using DotNetScript.Runtime;
using DotNetScript.Types.Reference;

namespace DotNetScript.Types
{
    internal struct ScriptCallArguments
    {
        private readonly object[] _arguments;

        public object[] Arguments => _arguments; 

        public ScriptCallArguments(ScriptMethodBase scriptMethod)
        {
            _arguments = new object[scriptMethod.MethodDefinition.Parameters.Count];

            for (var i = _arguments.Length - 1; i >= 0; i--)
            {
                var arg = RuntimeContext.Current.PopFromStack();
                var @ref = arg as IScriptReference;
                if (@ref != null)
                    arg = @ref.Value;

                var scriptObjectArg = arg as ScriptObject;

                if (scriptObjectArg != null && scriptMethod.ParamTypes[i].IsHost)
                    arg = scriptObjectArg.HostInstance;

                if (scriptObjectArg == null && !scriptMethod.ParamTypes[i].IsHost)
                    arg = ScriptObject.FromHostObject(arg);
                    
                _arguments[i] = arg;
            }
        }
    }
}
