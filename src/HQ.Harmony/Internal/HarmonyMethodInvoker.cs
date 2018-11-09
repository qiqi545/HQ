using System;
using System.Reflection;
using HQ.Remix;

namespace HQ.Harmony
{
    internal sealed class HarmonyMethodInvoker : MethodInvokerBase
    {
        private readonly IDependencyResolver _inner;

        public HarmonyMethodInvoker(IDependencyResolver inner)
        {
            _inner = inner;
        }

        public override object ResolveType(Type serviceType)
        {
            return _inner.Resolve(serviceType);
        }

        public override DelegateBuildStrategy BuildStrategyFor(MethodInfo method, ParameterInfo[] parameters)
        {
            return DelegateBuildStrategy.ObjectExecutor;
        }
    }
}
