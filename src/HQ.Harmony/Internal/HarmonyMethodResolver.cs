using System;
using HQ.Remix;

namespace HQ.Harmony
{
    internal sealed class HarmonyMethodResolver : MethodResolverBase
    {
        private readonly IDependencyResolver _inner;

        public HarmonyMethodResolver(IDependencyResolver inner)
        {
            _inner = inner;
        }

        public override object ResolveType(Type serviceType)
        {
            return _inner.Resolve(serviceType);
        }
    }
}