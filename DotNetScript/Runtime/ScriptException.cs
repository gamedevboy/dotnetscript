using System;
using System.Runtime.Serialization;

namespace DotNetScript.Runtime
{
    [Serializable]
    public class ScriptException : Exception
    {
        public ScriptException()
        {
        }

        public ScriptException(string message) : base(message)
        {
        }

        public ScriptException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ScriptException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}