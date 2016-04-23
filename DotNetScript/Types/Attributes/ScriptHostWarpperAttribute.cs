using System;

namespace DotNetScript.Types.Attributes
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ScriptHostWarpperAttribute : Attribute
    {
        public Type HostType { get; }

        public ScriptHostWarpperAttribute(Type hostType)
        {
            HostType = hostType;
        }
    }
}
