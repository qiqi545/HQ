using System;
using System.Reflection;

namespace HQ.Remix
{
    public abstract class MethodInvokerBase : IMethodInvoker
    {
        public abstract object ResolveType(Type serviceType);
        public abstract DelegateBuildStrategy BuildStrategyFor(MethodInfo method, ParameterInfo[] parameters);

        public object InvokeMethod(Type serviceType, string name)
        {
            var implementation = ResolveType(serviceType);
            if (implementation == null)
                return null;

            var method = MethodFactory.Default.GetOrCacheMethodForTypeAndName(serviceType, name);
            var parameters = MethodFactory.Default.GetOrCacheParametersForMethod(method);

            var args = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var arg = ResolveType(parameters[i].ParameterType);
                if (arg == null)
                    return null;
                args[i] = arg;
            }

            var handler = HandlerFactory.Default.GetOrCacheHandlerFromMethod(serviceType, method, BuildStrategyFor(method, parameters));

            return handler?.Invoke(implementation, args);
        }

        public object InvokeMethod<T>(string name) where T : class
        {
            return InvokeMethod(typeof(T), name);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) { }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
