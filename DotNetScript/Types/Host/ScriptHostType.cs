using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetScript.Runtime;
using Mono.Cecil;

namespace DotNetScript.Types.Host
{
    internal class ScriptHostType : ScriptType
    {
        private readonly List<ScriptType> _genericTypes = new List<ScriptType>();

        public override IReadOnlyList<ScriptType> GenericTypes => _genericTypes;
        public override Type HostType { get; }

        private Type _warpperedType;
        public override Type WarpperedType => _warpperedType;

        public ScriptHostType(TypeDefinition typeDef, ScriptAssembly scriptAssembly, params ScriptType[] genericTypes)
            : base(typeDef, scriptAssembly)
        {
            Debug.Assert(genericTypes.All(_=>_ != null));
            _genericTypes.AddRange(genericTypes);
            HostType = GetHostType();
        }

        public override ScriptType MakeGeneric(params ScriptType[] genericTypes)
        {
            return new ScriptHostType(TypeDefinition, ScriptAssembly, genericTypes).Initialize();
        }

        protected override Type GetHostType()
        {
            var typeName = $"{TypeDefinition.FullName}, {TypeDefinition.Module.Assembly.FullName}";

            if (TypeDefinition.IsNested)
                typeName = typeName.Replace('/', '+');

            var hostType = Type.GetType(typeName);
            var warpperType = ScriptContext.GetHostWarpperType(hostType);
            if (warpperType != null)
            {
                _warpperedType = hostType;
                hostType = warpperType;
            }

            if (hostType != null && GenericTypes.Count != 0)
                hostType = hostType.MakeGenericType(_genericTypes.Select(_ => _.HostType).ToArray());

            return hostType;
        }
    }
}
