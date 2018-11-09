using System;
using System.Reflection;

namespace HQ.Remix
{
    public interface IMethodResolver : IDisposable
    {
        MethodInfo ResolveMethod(Type serviceType, string name);
        MethodInfo ResolveMethod<T>(string name) where T : class;
        MethodInfo ResolveMethod(string serviceTypeName, string name);
    }
}
