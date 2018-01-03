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

namespace hq.compiler
{
    public class DefaultAssemblyBuilder : IAssemblyBuilder
    {
        private readonly AssemblyLoadContext _context;
        private readonly IEnumerable<IMetadataReferenceResolver> _resolvers;
	    private readonly ILogger<DefaultAssemblyBuilder> _logger;

	    public DefaultAssemblyBuilder(IAssemblyLoadContextProvider provider, IEnumerable<IMetadataReferenceResolver> resolvers, ILogger<DefaultAssemblyBuilder> logger = null)
        {
            _resolvers = resolvers;
	        _logger = logger;
	        _context = provider.Get();
        }

        public Assembly CreateInMemory(string code, params Assembly[] dependencies)
        {
	        var compilation = CreateCompilation(code, dependencies);

	        using (var peStream = new MemoryStream())
            {
                using (var pdbStream = new MemoryStream())
                {
                    EmitResult result = compilation.Emit(peStream, pdbStream);
					IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
					foreach(var failure in failures)
						_logger?.LogError(JsonConvert.SerializeObject(failure));

                    peStream.Seek(0, SeekOrigin.Begin);
                    var assembly = _context.LoadFromStream(peStream, pdbStream);
                    return assembly;
                }
            }
        }

	    public Assembly CreateOnDisk(string code, string outputPath, string pdbPath = null, params Assembly[] dependencies)
	    {
			var compilation = CreateCompilation(code, dependencies);

		    EmitResult result = compilation.Emit(outputPath, pdbPath);
		    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
		    foreach (var failure in failures)
			    _logger?.LogError(JsonConvert.SerializeObject(failure));

			Assembly assembly = Assembly.LoadFile(outputPath);
		    return assembly;
		}

		private CSharpCompilation CreateCompilation(string code, Assembly[] dependencies)
	    {
		    var references = ResolveReferences(dependencies);
		    var syntaxTree = CSharpSyntaxTree.ParseText(code);

		    CSharpCompilation compilation = CSharpCompilation.Create(
			    $"{Guid.NewGuid()}.dll",
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
            {
                foreach (var resolver in _resolvers)
                {
                    var reference = resolver.Resolve(dependency);
                    if (reference != null)
                        references.Add(reference);
                }
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