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
using Microsoft.AspNetCore.Http;

namespace HQ.Extensions.DependencyInjection.AspNetCore
{
	public class HttpAccessorExtension : IResolverExtension
	{
		public bool CanResolve(Lifetime lifetime)
		{
			return lifetime == Lifetime.Request;
		}

		public Func<T> Memoize<T>(IDependencyResolver host, Func<T> f)
		{
			return () =>
			{
				var accessor = host.Resolve<IHttpContextAccessor>();
				if (accessor?.HttpContext == null)
				{
					return f(); // always new
				}

				var cache = accessor.HttpContext.Items;
				var cacheKey = f.ToString();
				if (cache.TryGetValue(cacheKey, out var item))
				{
					return (T) item; // got it
				}

				item = f(); // need it
				cache.Add(cacheKey, item);
				if (item is IDisposable disposable)
				{
					accessor.HttpContext.Response.RegisterForDispose(disposable);
				}

				return (T) item;
			};
		}

		public Func<IDependencyResolver, T> Memoize<T>(IDependencyResolver host, Func<IDependencyResolver, T> f)
		{
			return r =>
			{
				var accessor = r.Resolve<IHttpContextAccessor>();
				if (accessor?.HttpContext == null)
				{
					return f(host); // always new
				}

				var cache = accessor.HttpContext.Items;
				var cacheKey = f.ToString();
				if (cache.TryGetValue(cacheKey, out var item))
				{
					return (T) item; // got it
				}

				item = f(host); // need it
				cache.Add(cacheKey, item);
				if (item is IDisposable disposable)
				{
					accessor.HttpContext.Response.RegisterForDispose(disposable);
				}

				return (T) item;
			};
		}
	}
}