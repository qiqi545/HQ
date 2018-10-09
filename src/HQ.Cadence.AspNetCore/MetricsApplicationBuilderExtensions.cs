// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using HQ.Cadence.AspNetCore.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Cadence.AspNetCore
{
	public static class MetricsApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseMetrics(this IApplicationBuilder app)
		{
			var reporters = app.ApplicationServices.GetServices<IMetricsReporter>();
			foreach (var reporter in reporters)
				reporter.Start();

			app.UseMiddleware<MetricsMiddleware>(app.ApplicationServices
				.GetRequiredService<IOptions<MetricsOptions>>());

			return app;
		}
	}
}