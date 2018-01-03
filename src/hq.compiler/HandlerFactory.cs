using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace hq.compiler
{
    public class HandlerFactory
    {
        private readonly IAssemblyBuilder _builder;
        private readonly IEnumerable<Assembly> _defaultDependencies;

        public HandlerFactory(IAssemblyBuilder builder, IEnumerable<Assembly> defaultDependencies)
        {
            _builder = builder;
            _defaultDependencies = defaultDependencies ?? Runtime.GetRuntimeAssemblies();
        }

        public HandlerFactory(IAssemblyBuilder builder, params Assembly[] defaultDependencies)
        {
            _builder = builder;
            _defaultDependencies = defaultDependencies ?? Runtime.GetRuntimeAssemblies();
        }

		public static HandlerFactory Default = new HandlerFactory(new DefaultAssemblyBuilder(new AssemblyLoadContextProvider(), new IMetadataReferenceResolver[]
	    {
		    new DefaultMetadataReferenceResolver()
	    }));

	    public Assembly BuildAssemblyInMemory(HandlerInfo info, params Assembly[] dependencies)
	    {
			var code = info.Code ?? NoCodeHandler;
		    Assembly[] mergedDependencies = _defaultDependencies.Union(dependencies).Distinct().ToArray();
		    Assembly a = _builder.CreateInMemory(code, mergedDependencies);
		    return a;
	    }

	    public Assembly BuildAssemblyOnDisk(HandlerInfo info, string outputPath, string pdbPath = null, params Assembly[] dependencies)
	    {
		    var code = info.Code ?? NoCodeHandler;
		    Assembly[] mergedDependencies = _defaultDependencies.Union(dependencies).Distinct().ToArray();
		    Assembly a = _builder.CreateOnDisk(code, outputPath, pdbPath, mergedDependencies);
		    return a;
	    }

		public Type BuildType(HandlerInfo info, Assembly[] dependencies)
	    {
		    var a = BuildAssemblyInMemory(info, dependencies);
		    string entrypoint = info.Entrypoint ?? $"{info.Namespace ?? "hq"}.Main";
		    Type t = a?.GetType(entrypoint);
		    return t;
	    }

		public MethodInfo BuildHandler(HandlerInfo info, params Assembly[] dependencies)
		{
			var t = BuildType(info, dependencies);
			string function = info.Function ?? "Execute";
			MethodInfo h = t?.GetMethod(function, BindingFlags.Public | BindingFlags.Static);
            return h;
        }

	    private const string NoCodeHandler = @"
namespace hq
{ 
    public class Main
    { 
        public static string Execute()
        { 
            return ""Hello, World!"";
        }
    }
}";
    }
}
