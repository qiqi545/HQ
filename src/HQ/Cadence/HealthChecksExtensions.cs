// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

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