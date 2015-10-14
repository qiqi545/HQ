using System.Collections.Generic;
using System.Reflection;

namespace container
{
    public interface IContainer : IDependencyResolver, IDependencyRegistrar
    {
        IEnumerable<Assembly> LoadedAssemblies { get; }
    }
}