using System.Runtime.Loader;
using hq.compiler;

namespace snippets
{
    public class DefaultAssemblyLoadContextProvider : IAssemblyLoadContextProvider
    {
        public AssemblyLoadContext Get()
        {
            return AssemblyLoadContext.Default;
        }
    }
}