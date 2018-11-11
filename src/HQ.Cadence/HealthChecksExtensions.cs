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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Cadence
{
    public static class HealthChecksExtensions
    {
        public static IHealthChecksBuilder AddHealthChecks(this IMetricsBuilder metricsBuilder,
            Action<IHealthChecksBuilder> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var services = metricsBuilder.Services;

            var store = new InMemoryHealthChecksStore();
            var host = new HealthChecksHost(store);
            var registry = new InMemoryHealthChecksRegistry();
            registry.Add(host);

            services.TryAdd(ServiceDescriptor.Singleton<IHealthChecksRegistry>(r => registry));
            services.TryAdd(ServiceDescriptor.Singleton<IHealthChecksStore, InMemoryHealthChecksStore>(r => store));
            services.TryAdd(ServiceDescriptor.Singleton<IHealthChecksHost, HealthChecksHost>(r => host));
            services.TryAdd(ServiceDescriptor.Singleton<IHealthChecksStore>(r => new InMemoryHealthChecksStore()));

            var healthChecksBuilder = new HealthChecksBuilder(metricsBuilder.Services);

            configure(healthChecksBuilder);

            return healthChecksBuilder;
        }

        public static IHealthChecksBuilder AddHealthCheck<T>(this IHealthChecksBuilder builder, Func<bool> func)
        {
            return builder;
        }
    }
}
