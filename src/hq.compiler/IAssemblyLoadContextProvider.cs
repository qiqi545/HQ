using System.Runtime.Loader;

namespace hq.compiler
{
    public interface IAssemblyLoadContextProvider
    {
        AssemblyLoadContext Get();
    }
}