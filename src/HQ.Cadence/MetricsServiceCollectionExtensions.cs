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
using HQ.Cadence.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace HQ.Cadence
{
    public static class MetricsServiceCollectionExtensions
    {
        public static IServiceCollection AddMetrics(this IServiceCollection services)
        {
            return AddMetrics(services, builder => { });
        }

        public static IServiceCollection AddMetrics(this IServiceCollection services, IConfiguration configuration)
        {
            return AddMetrics(services, configuration.Bind);
        }

        public static IServiceCollection AddMetrics(this IServiceCollection services, IConfiguration configuration,
            Action<IMetricsBuilder> configure)
        {
            return AddMetrics(services, builder =>
            {
                var options = new MetricsOptions();
                configuration.Bind(options);
                configure(builder);
            });
        }

        public static IServiceCollection AddMetrics(this IServiceCollection services, Action<IMetricsBuilder> configure)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddOptions();

            var store = new InMemoryMetricsStore();
            var host = new MetricsHost(store);
            var registry = new InMemoryMetricsRegistry {host};

            services.TryAdd(ServiceDescriptor.Singleton<IMetricsRegistry>(r => registry));
            services.TryAdd(ServiceDescriptor.Singleton<IMetricsStore, InMemoryMetricsStore>(r => store));
            services.TryAdd(ServiceDescriptor.Singleton<IMetricsHost, MetricsHost>(r => host));
            services.TryAdd(ServiceDescriptor.Singleton(typeof(IMetricsHost<>), typeof(MetricsHost<>)));

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<MetricsOptions>>(
                new DefaultMetricsConfigureOptions()));

            configure(new MetricsBuilder(services));

            return services;
        }
    }
}
