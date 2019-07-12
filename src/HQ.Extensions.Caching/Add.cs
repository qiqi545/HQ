using System;
using HQ.Extensions.Caching.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Extensions.Caching
{
    public static class Add
    {
        public static IServiceCollection AddInProcessCache(this IServiceCollection services, Action<CacheOptions> configureAction = null)
        {
            services.AddOptions();

            if (configureAction != null)
                services.Configure(configureAction);

            services.TryAdd(ServiceDescriptor.Singleton<IMemoryCache, MemoryCache>());
            services.TryAdd(ServiceDescriptor.Singleton<ICache, InProcessCache>());

            return services;
        }

        public static IServiceCollection AddDistributedCache(this IServiceCollection services, Action<CacheOptions> configureAction = null)
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
