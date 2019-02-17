using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Mono.Cecil;

namespace HQ.Extensions.CodeGeneration.Internal.Extensions
{
    internal static class ReflectionExtensions
    {
        private static readonly IDictionary<string, AssemblyDefinition> AssemblyCache
            = new Dictionary<string, AssemblyDefinition>();

        private static readonly IDictionary<AssemblyDefinition, ModuleDefinition> ModuleCache
            = new Dictionary<AssemblyDefinition, ModuleDefinition>();

        private static readonly IDictionary<MethodInfo, MethodReference> MethodReferenceCache
            = new Dictionary<MethodInfo, MethodReference>();

        private static readonly IDictionary<MethodReference, MethodDefinition> MethodDefinitionCache
            = new Dictionary<MethodReference, MethodDefinition>();

        internal static MethodReference ToReference(this MethodInfo method)
        {
            try
            {
                if (MethodReferenceCache.TryGetValue(method, out var reference))
                    return reference;

                if (!AssemblyCache.TryGetValue(method.Module.Assembly.FullName, out var assembly))
                    AssemblyCache.Add(method.Module.Assembly.FullName, assembly = AssemblyDefinition.ReadAssembly(method.Module.Assembly.Location));

                if (!ModuleCache.TryGetValue(assembly, out var module))
                    ModuleCache.Add(assembly, module = ModuleDefinition.ReadModule(method.Module.FullyQualifiedName));

                MethodReferenceCache.Add(method, reference = module.ImportReference(method));
                return reference;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return null;
            }
        }

        internal static MethodDefinition ToDefinition(this MethodInfo method)
        {
            try
            {
                var reference = method.ToReference();

                if(!MethodDefinitionCache.TryGetValue(reference, out var definition))
                    MethodDefinitionCache.Add(reference, definition = reference.Resolve());

                return definition;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return null;
            }
        }
    }
}
