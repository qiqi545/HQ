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
using ActiveCaching;
using ActiveCaching.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Extensions.Caching
{
	public static class Add
	{
		public static IServiceCollection AddInProcessCache(this IServiceCollection services,
			Action<CacheOptions> configureAction = null)
		{
			services.AddOptions();

			if (configureAction != null)
				services.Configure(configureAction);

			services.TryAdd(ServiceDescriptor.Singleton<IMemoryCache, MemoryCache>());
			services.TryAdd(ServiceDescriptor.Singleton<ICache, InProcessCache>());

			return services;
		}

		public static IServiceCollection AddDistributedCache(this IServiceCollection services,
			Action<CacheOptions> configureAction = null)
		{
			services.AddOptions();

			if (configureAction != null)
				services.Configure(configureAction);

			services.TryAdd(ServiceDescriptor.Singleton<IDistributedCache, MemoryDistributedCache>());
			services.TryAdd(ServiceDescriptor.Singleton<ICache, DistributedCache>());

			return services;
		}
	}
}