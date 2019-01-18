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
    public abstract class MethodResolverBase : IMethodResolver
    {
        public MethodInfo ResolveMethod(Type serviceType, string name)
        {
            var implementation = ResolveType(serviceType);
            return implementation == null
                ? null
                : MethodFactory.Default.GetOrCacheMethodForTypeAndName(serviceType, name);
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract object ResolveType(Type serviceType);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}
