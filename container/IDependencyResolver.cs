using System;
using System.Collections.Generic;

namespace container
{
    public interface IDependencyResolver : IDisposable
    {
        T Resolve<T>() where T : class;
        T Resolve<T>(string name) where T : class;
        object Resolve(Type serviceType);
        object Resolve(Type serviceType, string name);
        IEnumerable<T> ResolveAll<T>() where T : class;
    }
}
