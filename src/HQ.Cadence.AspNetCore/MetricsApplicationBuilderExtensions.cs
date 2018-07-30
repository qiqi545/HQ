// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Cadence.AspNetCore
{
	public static class MetricsApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseMetricsReporting(this IApplicationBuilder app)
		{
			var reporters = app.ApplicationServices.GetServices<IMetricsReporter>();
			foreach (var reporter in reporters)
				reporter.Start();

			return app;
		}
	}
}