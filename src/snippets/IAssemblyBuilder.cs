using System.Reflection;

namespace snippets
{
    public interface IAssemblyBuilder
    {
        Assembly CreateInMemory(string assemblyName, string code, params Assembly[] dependencies);
	    Assembly CreateInMemory(string assemblyName, string code, params string[] dependencyLocations);
		Assembly CreateOnDisk(string assemblyName, string code, string outputPath, string pdbPath = null, params Assembly[] dependencies);
	    Assembly CreateOnDisk(string assemblyName, string code, string outputPath, string pdbPath = null, params string[] dependencyLocations);
	}
}