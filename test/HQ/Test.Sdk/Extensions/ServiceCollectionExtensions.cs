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
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Test.Sdk.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ReplaceWithSingleton<TService>(this IServiceCollection services,
            TService instance) where TService : class
        {
            return Replace(services, instance);
        }

        public static IServiceCollection ReplaceWithSingleton<TService, TImplementation>(
            this IServiceCollection services)
        {
            return Replace<TService, TImplementation>(services, ServiceLifetime.Singleton);
        }

        public static IServiceCollection ReplaceWithSingleton<TService>(this IServiceCollection services,
            Func<IServiceProvider, object> factory)
        {
            return Replace<TService>(services, factory, ServiceLifetime.Singleton);
        }

        public static IServiceCollection ReplaceWithScoped<TService, TImplementation>(this IServiceCollection services)
        {
            return Replace<TService, TImplementation>(services, ServiceLifetime.Scoped);
        }

        public static IServiceCollection ReplaceWithScoped<TService>(this IServiceCollection services,
            Func<IServiceProvider, object> factory)
        {
            return Replace<TService>(services, factory, ServiceLifetime.Scoped);
        }

        public static IServiceCollection ReplaceWithTransient<TService, TImplementation>(
            this IServiceCollection services)
        {
            return Replace<TService, TImplementation>(services, ServiceLifetime.Transient);
        }

        public static IServiceCollection ReplaceWithTransient<TService>(this IServiceCollection services,
            Func<IServiceProvider, object> factory)
        {
            return Replace<TService>(services, factory, ServiceLifetime.Transient);
        }

        public static bool TryReplaceExact<TService>(this IServiceCollection services, TService instance)
        {
            for (var i = 0; i < services.Count; i++)
                if (services[i].ServiceType == typeof(TService) && services[i].Lifetime == ServiceLifetime.Singleton)
                {
                    services[i] = new ServiceDescriptor(typeof(TService), instance);
                    return true;
                }

            return false;
        }

        public static bool TryReplaceExact<TService, TImplementation>(this IServiceCollection services)
        {
            for (var i = 0; i < services.Count; i++)
                if (services[i].ServiceType == typeof(TService) && services[i].ImplementationType != null)
                {
                    services[i] =
                        new ServiceDescriptor(typeof(TService), typeof(TImplementation), services[i].Lifetime);
                    return true;
                }

            return false;
        }

        public static bool TryReplaceExact<TService>(this IServiceCollection services,
            Func<IServiceProvider, object> factory)
        {
            for (var i = 0; i < services.Count; i++)
                if (services[i].ServiceType == typeof(TService) && services[i].ImplementationFactory != null)
                {
                    services[i] = new ServiceDescriptor(typeof(TService), factory, services[i].Lifetime);
                    return true;
                }

            return false;
        }

        private static IServiceCollection Replace<TService>(IServiceCollection services, TService instance)
            where TService : class
        {
            for (var i = 0; i < services.Count; i++)
                if (services[i].ServiceType == typeof(TService))
                {
                    services[i] = new ServiceDescriptor(typeof(TService), instance);
                    return services;
                }

            services.AddSingleton(instance);
            return services;
        }

        private static IServiceCollection Replace<TService, TImplementation>(IServiceCollection services,
            ServiceLifetime lifetime)
        {
            for (var i = 0; i < services.Count; i++)
                if (services[i].ServiceType == typeof(TService))
                {
                    services[i] = new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime);
                    return services;
                }

            services.Add(ServiceDescriptor.Describe(typeof(TService), typeof(TImplementation), lifetime));
            return services;
        }

        private static IServiceCollection Replace<TService>(IServiceCollection services,
            Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
        {
            for (var i = 0; i < services.Count; i++)
                if (services[i].ServiceType == typeof(TService))
                {
                    services[i] = new ServiceDescriptor(typeof(TService), factory, lifetime);
                    return services;
                }

            services.Add(ServiceDescriptor.Describe(typeof(TService), factory, lifetime));
            return services;
        }
    }
}
