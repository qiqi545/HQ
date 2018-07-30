// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using HQ.Cadence.Internal;
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

	    public static IServiceCollection AddMetrics(this IServiceCollection services, Action<IMetricsBuilder> configure)
	    {
		    if (services == null)
			    throw new ArgumentNullException(nameof(services));

		    services.AddOptions();
			
		    var store = new InMemoryMetricsStore();
			var host = new MetricsHost(store);
		    var registry = new InMemoryMetricsRegistry();
		    registry.Add(host);
			
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