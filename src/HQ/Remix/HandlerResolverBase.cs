using System;
using System.Reflection;

namespace HQ.Remix
{
    public abstract class HandlerResolverBase : IHandlerResolver
    {
        public abstract MethodInfo ResolveMethod(Type serviceType, string name);
        public abstract DelegateBuildStrategy BuildStrategyFor(MethodInfo method, ParameterInfo[] parameters);

        public Handler ResolveHandler(MethodInfo method)
        {
            return HandlerFactory.Default.GetOrCacheHandlerFromMethod(method.DeclaringType, method,
                BuildStrategyFor(method, method.GetParameters()));
        }

        public Handler ResolveHandler(Type serviceType, string name)
        {
            var method = ResolveMethod(serviceType, name);
            return method == null ? null : ResolveHandler(method);
        }

        public Handler ResolveHandler<T>(string name) where T : class
        {
            return ResolveHandler(typeof(T), name);
        }
    }
}
