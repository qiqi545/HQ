// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Cadence.Reporters.Console
{
	public static class ConsoleReporterExtensions
	{
		public static IMetricsBuilder AddConsoleReporter(this IMetricsBuilder builder)
		{
			var descriptor = ServiceDescriptor.Singleton<IMetricsReporter, ConsoleReporter>();
			builder.Services.TryAddEnumerable(descriptor);
			return builder;
		}

		public static IMetricsBuilder AddConsoleReporter(this IMetricsBuilder builder,
			Action<ConsoleReporterOptions> configure)
		{
			if (configure == null)
				throw new ArgumentNullException(nameof(configure));

			builder.AddConsoleReporter();
			builder.Services.Configure(configure);

			return builder;
		}
	}
}