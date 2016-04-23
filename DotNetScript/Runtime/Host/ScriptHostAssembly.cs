using Mono.Cecil;

namespace DotNetScript.Runtime.Host
{
    public class ScriptHostAssembly : ScriptAssembly
    {
        private readonly ScriptHostTypeSystem _typeSystem;
        public override ScriptTypeSystem TypeSystem => _typeSystem;

        public ScriptHostAssembly(AssemblyDefinition assemblyDefinition)
            : base(assemblyDefinition)
        {
            _typeSystem = new ScriptHostTypeSystem(this);
        }
    }
}
