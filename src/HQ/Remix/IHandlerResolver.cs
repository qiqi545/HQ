using System;
using System.Reflection;

namespace HQ.Remix
{
    public interface IHandlerResolver
    {
        Handler ResolveHandler(MethodInfo method);
        Handler ResolveHandler(Type serviceType, string name);
        Handler ResolveHandler<T>(string name) where T : class;
    }
}
