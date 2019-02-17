#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HQ.CodeGeneration
{
    public class AssemblyBuilder : IAssemblyBuilder
    {
        public static Lazy<IAssemblyBuilder> Default = new Lazy<IAssemblyBuilder>(() =>
        {
            var loadContextProvider = new DefaultAssemblyLoadContextProvider();
            var metadataReferenceResolver = new DefaultMetadataReferenceResolver();
            return new AssemblyBuilder(loadContextProvider, new[] {metadataReferenceResolver});
        });

        private readonly AssemblyLoadContext _context;
        private readonly ILogger<AssemblyBuilder> _logger;
        private readonly IEnumerable<IMetadataReferenceResolver> _resolvers;

        public AssemblyBuilder(IAssemblyLoadContextProvider provider, IEnumerable<IMetadataReferenceResolver> resolvers,
            ILogger<AssemblyBuilder> logger = null)
        {
            _resolvers = resolvers;
            _logger = logger;
            _context = provider.Get();
        }

        public Assembly CreateInMemory(string assemblyName, string code, params Assembly[] dependencies)
        {
            var compilation = CreateCompilation(assemblyName, code, dependencies);

            using (var peStream = new MemoryStream())
            {
                using (var pdbStream = new MemoryStream())
                {
                    var result = compilation.Emit(peStream, pdbStream);
                    MaybeLogWarningsAndErrors(result);

                    peStream.Seek(0, SeekOrigin.Begin);
                    var assembly = _context.LoadFromStream(peStream, pdbStream);
                    return assembly;
                }
            }
        }

        public Assembly CreateInMemory(string assemblyName, string code, params string[] dependencyLocations)
        {
            var compilation = CreateCompilation(assemblyName, code, dependencyLocations);

            using (var peStream = new MemoryStream())
            {
                using (var pdbStream = new MemoryStream())
                {
                    var result = compilation.Emit(peStream, pdbStream);
                    MaybeLogWarningsAndErrors(result);

                    peStream.Seek(0, SeekOrigin.Begin);
                    var assembly = _context.LoadFromStream(peStream, pdbStream);
                    return assembly;
                }
            }
        }

        public Assembly CreateOnDisk(string assemblyName, string code, string outputPath, string pdbPath = null,
            params Assembly[] dependencies)
        {
            var compilation = CreateCompilation(assemblyName, code, dependencies);

            var result = compilation.Emit(outputPath, pdbPath);
            MaybeLogWarningsAndErrors(result);

            var assembly = Assembly.LoadFile(outputPath);
            return assembly;
        }

        public Assembly CreateOnDisk(string assemblyName, string code, string outputPath, string pdbPath = null,
            params string[] dependencyLocations)
        {
            var compilation = CreateCompilation(assemblyName, code, dependencyLocations);

            var result = compilation.Emit(outputPath, pdbPath);
            MaybeLogWarningsAndErrors(result);

            var assembly = Assembly.LoadFile(outputPath);
            return assembly;
        }

        private void MaybeLogWarningsAndErrors(EmitResult result)
        {
            var warnings = result.Diagnostics.Where(diagnostic =>
                !diagnostic.IsSuppressed && !diagnostic.IsWarningAsError &&
                diagnostic.Severity == DiagnosticSeverity.Warning);

            foreach (var warning in warnings)
                if (_logger?.IsEnabled(LogLevel.Warning) == true)
                    _logger?.LogWarning(JsonConvert.SerializeObject(warning));

            var errors = result.Diagnostics.Where(diagnostic =>
                !diagnostic.IsSuppressed && diagnostic.IsWarningAsError ||
                diagnostic.Severity == DiagnosticSeverity.Error);
            foreach (var error in errors)
                if (_logger?.IsEnabled(LogLevel.Error) == true)
                    _logger?.LogError(JsonConvert.SerializeObject(error));
        }

        private CSharpCompilation CreateCompilation(string assemblyName, string code,
            IEnumerable<Assembly> dependencies)
        {
            var references = ResolveReferences(dependencies);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] {syntaxTree},
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            return compilation;
        }

        private CSharpCompilation CreateCompilation(string assemblyName, string code,
            IEnumerable<string> dependencyLocations)
        {
            var references = ResolveReferences(dependencyLocations);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] {syntaxTree},
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            return compilation;
        }

        private IEnumerable<MetadataReference> ResolveReferences(IEnumerable<Assembly> dependencies)
        {
            var compiled = GetCompileTimeReferences();
            var references = new HashSet<MetadataReference>(compiled);
            foreach (var dependency in dependencies)
            foreach (var resolver in _resolvers)
            {
                var reference = resolver.Resolve(dependency);
                if (reference != null)
                    references.Add(reference);
            }

            return references;
        }

        private IEnumerable<MetadataReference> ResolveReferences(IEnumerable<string> assemblyLocations)
        {
            var compiled = GetCompileTimeReferences();
            var references = new HashSet<MetadataReference>(compiled);
            foreach (var dependency in assemblyLocations)
            foreach (var resolver in _resolvers)
            {
                var reference = resolver.Resolve(dependency);
                if (reference != null)
                    references.Add(reference);
            }

            return references;
        }

        private static IEnumerable<MetadataReference> GetCompileTimeReferences()
        {
            return
                from library in DependencyContext.Default.CompileLibraries
                from path in library.ResolveReferencePaths()
                select AssemblyMetadata.CreateFromFile(path)
                into assembly
                select assembly.GetReference();
        }
    }
}
