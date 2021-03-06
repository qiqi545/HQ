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

using HQ.Platform.Api.Metrics.Internal;
using Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Api.Metrics
{
	public static class Use
	{
		public static IApplicationBuilder UseMetrics(this IApplicationBuilder app, PathString path)
		{
			var reporters = app.ApplicationServices.GetServices<IMetricsReporter>();
			foreach (var reporter in reporters)
			{
				reporter.InitializeAsync();
			}

			app.UseMiddleware<MetricsMiddleware>(path,
				app.ApplicationServices.GetRequiredService<IOptions<MetricsOptions>>());
			return app;
		}
	}
}