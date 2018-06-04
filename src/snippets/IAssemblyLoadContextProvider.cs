using System.Runtime.Loader;

namespace snippets
{
    public interface IAssemblyLoadContextProvider
    {
        AssemblyLoadContext Get();
    }
}