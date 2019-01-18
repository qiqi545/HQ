#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Reflection;

namespace HQ.CodeGeneration
{
    public abstract class MethodInvokerBase : IMethodInvoker
    {
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

            var handler =
                HandlerFactory.Default.GetOrCacheHandlerFromMethod(serviceType, method,
                    BuildStrategyFor(method, parameters));

            return handler?.Invoke(implementation, args);
        }

        public object InvokeMethod<T>(string name) where T : class
        {
            return InvokeMethod(typeof(T), name);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract object ResolveType(Type serviceType);
        public abstract DelegateBuildStrategy BuildStrategyFor(MethodInfo method, ParameterInfo[] parameters);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}
