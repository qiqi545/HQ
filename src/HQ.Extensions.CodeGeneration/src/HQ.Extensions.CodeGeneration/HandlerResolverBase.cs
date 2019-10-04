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

namespace HQ.Extensions.CodeGeneration
{
	public abstract class HandlerResolverBase : IHandlerResolver
	{
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

		public abstract MethodInfo ResolveMethod(Type serviceType, string name);
		public abstract DelegateBuildStrategy BuildStrategyFor(MethodInfo method, ParameterInfo[] parameters);
	}
}