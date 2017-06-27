using System.Reflection;

namespace hq.compiler
{
    public interface IAssemblyBuilder
    {
        Assembly Create(string code, params Assembly[] assemblies);
    }
}