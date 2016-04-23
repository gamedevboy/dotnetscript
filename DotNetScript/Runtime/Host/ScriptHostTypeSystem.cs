using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetScript.Runtime;
using DotNetScript.Types;
using DotNetScript.Types.Host;
using Mono.Cecil;

namespace DotNetScript.Runtime.Host
{
    internal class ScriptHostTypeSystem : ScriptTypeSystem
    {
        public ScriptHostTypeSystem(ScriptAssembly scriptAssembly) 
            : base(scriptAssembly)
        {
        }

        protected override ScriptType CreateScriptType(TypeDefinition typeDef)
        {
            return new ScriptHostType(typeDef, ScriptAssembly);
        }

        internal override ScriptType GetType(TypeDefinition typeDef)
        {
            var hostScriptType = (ScriptHostType) base.GetType(typeDef);
            hostScriptType?.Initialize();
            return hostScriptType;
        }
    }
}
