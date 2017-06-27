using System.Runtime.Loader;

namespace hq.compiler
{
    public class AssemblyLoadContextProvider : IAssemblyLoadContextProvider
    {
        public AssemblyLoadContext Get()
        {
            return AssemblyLoadContext.Default;
        }
    }
}