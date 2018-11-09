using System;
using System.Collections.Generic;

namespace HQ.Remix
{
    public interface ITypeResolver : IDisposable
    {
        IEnumerable<Type> ResolveByExample<TInterface>();
    }
}
