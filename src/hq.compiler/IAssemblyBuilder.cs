using System.Reflection;

namespace hq.compiler
{
    public interface IAssemblyBuilder
    {
        Assembly CreateInMemory(string code, params Assembly[] assemblies);
	    Assembly CreateOnDisk(string code, string outputPath, string pdbPath = null, params Assembly[] dependencies);
    }
}