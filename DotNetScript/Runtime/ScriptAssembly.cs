using System.IO;
using Mono.Cecil;

namespace DotNetScript.Runtime
{
    public class ScriptAssembly
    {
        private readonly AssemblyDefinition _rawAssemblyDefinition;
        internal AssemblyDefinition AssemblyDefinition => _rawAssemblyDefinition;

        private readonly ScriptTypeSystem _typeSystem;

        public virtual ScriptTypeSystem TypeSystem => _typeSystem;

        protected ScriptAssembly(AssemblyDefinition assemblyDefinition)
        {
            _rawAssemblyDefinition = assemblyDefinition;
        }

        public ScriptAssembly(Stream rawAssebmlyStream) 
            : this(AssemblyDefinition.ReadAssembly(rawAssebmlyStream))
        {
            _typeSystem = new ScriptTypeSystem(this);
        }
    }
}

