using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using DotNetScript.Runtime.Host;
using DotNetScript.Types;
using DotNetScript.Types.Attributes;
using Mono.Cecil;

namespace DotNetScript.Runtime
{
    public class ScriptContext
    {
        private static readonly ConcurrentDictionary<ModuleDefinition, ScriptAssembly > ScriptAssemblies = new ConcurrentDictionary<ModuleDefinition, ScriptAssembly>();
        private static readonly Dictionary<Type, Type> HostWapperTypes;

        static ScriptContext()
        {
            var types = Thread.GetDomain().GetAssemblies().SelectMany(_ => _.GetTypes().Where(t => t.GetCustomAttribute<ScriptHostWarpperAttribute>() != null));
            HostWapperTypes = types.ToDictionary(_ => _.GetCustomAttribute<ScriptHostWarpperAttribute>().HostType);
        }

        internal static ScriptType GetType(TypeReference typeRef)
        {
            if (typeRef == null)
                return null;

            var typeDef = typeRef.Resolve();
            if (typeDef == null)
                return null;

            var module = typeDef.Module;
            var scriptAssembly = ScriptAssemblies.GetOrAdd(module, _ => new ScriptHostAssembly(module.Assembly));
            return scriptAssembly.TypeSystem.GetType(typeRef);
        }

        public static void Load(ScriptAssembly scriptAssembly)
        {
            ScriptAssemblies.GetOrAdd(scriptAssembly.AssemblyDefinition.MainModule, scriptAssembly);
        }

        internal static ScriptAssembly Get(ModuleDefinition module)
        {
            ScriptAssembly scriptAssembly;
            ScriptAssemblies.TryGetValue(module, out scriptAssembly);
            return scriptAssembly;
        }

        internal static Type GetHostWarpperType(Type hostType)
        {
            Type ret;
            HostWapperTypes.TryGetValue(hostType, out ret);
            return ret;
        }

        internal static ScriptType GetType(Type type)
        {
            var module = ScriptAssemblies.FirstOrDefault().Key;
            return GetType(module.Import(type));
        }

        internal static bool IsHost(ModuleDefinition module)
        {
            ScriptAssembly scriptAssembly;

            if (ScriptAssemblies.TryGetValue(module, out scriptAssembly))
                return scriptAssembly is ScriptHostAssembly;

            return false;
        }

        internal static ScriptMethodBase GetMethod(MethodReference methodRef)
        {
            return Get(methodRef.Module).TypeSystem.GetType(methodRef.DeclaringType).GetMethod(methodRef);
        }
    }
}
