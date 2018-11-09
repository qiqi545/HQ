// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Cadence.Reporters.Console
{
	public static class LogReporterExtensions
	{
		public static IMetricsBuilder AddLogReporter(this IMetricsBuilder builder)
		{
			builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetricsReporter, LogReporter>());
			return builder;
		}

		public static IMetricsBuilder AddLogReporter(this IMetricsBuilder builder, Action<LogReporterOptions> configure)
		{
			if (configure == null)
				throw new ArgumentNullException(nameof(configure));

			builder.AddLogReporter();
			builder.Services.Configure(configure);

			return builder;
		}
	}
}