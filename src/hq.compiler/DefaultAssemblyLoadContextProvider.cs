using System.Runtime.Loader;

namespace hq.compiler
{
    public class DefaultAssemblyLoadContextProvider : IAssemblyLoadContextProvider
    {
        public AssemblyLoadContext Get()
        {
            return AssemblyLoadContext.Default;
        }
    }
}