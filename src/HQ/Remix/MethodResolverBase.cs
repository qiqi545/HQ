using System;
using System.Reflection;

namespace HQ.Remix
{
    public abstract class MethodResolverBase : IMethodResolver
    {
        public abstract object ResolveType(Type serviceType);
        
        public MethodInfo ResolveMethod(Type serviceType, string name)
        {
            var implementation = ResolveType(serviceType);
            return implementation == null ? null : MethodFactory.Default.GetOrCacheMethodForTypeAndName(serviceType, name);
        }

        public MethodInfo ResolveMethod<T>(string name) where T : class
        {
            return ResolveMethod(typeof(T), name);
        }

        public MethodInfo ResolveMethod(string serviceTypeName, string name)
        {
            var serviceType = Type.GetType(serviceTypeName);
            return serviceType == null ? null : ResolveMethod(serviceType, name);
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
