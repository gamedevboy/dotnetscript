using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotNetScript.Types;
using Mono.Cecil;

namespace DotNetScript.Runtime
{
    public class ScriptTypeSystem
    {
        private readonly ScriptAssembly _scriptAssembly;
        internal ScriptAssembly ScriptAssembly => _scriptAssembly;

        private readonly ConcurrentDictionary<TypeDefinition, ScriptType> _scriptTypes = new ConcurrentDictionary<TypeDefinition, ScriptType>();
        private readonly Dictionary<string, TypeDefinition> _types;

        public ScriptTypeSystem(ScriptAssembly scriptAssembly)
        {
            _scriptAssembly = scriptAssembly;

            var types = _scriptAssembly.AssemblyDefinition.MainModule.GetTypes();

            if (types != null)
                _types = types.ToDictionary(_ => _.FullName);
        }

        internal virtual ScriptType GetType(TypeDefinition typeDef)
        {
            return typeDef == null ? null : _scriptTypes.GetOrAdd(typeDef, CreateScriptType);
        }

        protected virtual ScriptType CreateScriptType(TypeDefinition typeDef)
        {
            return new ScriptType(typeDef, _scriptAssembly);
        }

        internal ScriptType GetType(TypeReference typeRef)
        {
            if (typeRef == null)
                return null;

            if(!typeRef.IsGenericInstance)
                return GetType(typeRef.Resolve());

            var genericTypeRef = (GenericInstanceType) typeRef;

            if( genericTypeRef.GenericArguments.Any(_=>_.IsGenericParameter) )
                return GetType(typeRef.Resolve());

            return GetType(typeRef.Resolve()).MakeGeneric(genericTypeRef.GenericArguments.Select(GetType).ToArray());
        }

        public ScriptType GetType(string typeName)
        {
            TypeDefinition typeDef;
            return _types.TryGetValue(typeName, out typeDef) ? GetType(typeDef) : null;
        }

        internal ScriptObject CreateInstance(ScriptType scriptType, params object[] param)
        {
            var constructor = scriptType.GetConsturctor(param.Select(_ => new ScriptType(null, null)).ToArray());

            var scriptObject = new ScriptObject(scriptType);

            constructor?.Invoke(scriptObject, param);

            return scriptObject;
        }
    }
}
