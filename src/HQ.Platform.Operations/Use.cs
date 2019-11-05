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

using System.Threading.Tasks;
using HQ.Common;
using HQ.Extensions.Metrics.Reporters.ServerTiming;
using HQ.Platform.Operations.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace HQ.Platform.Operations
{
	public static class Use
	{
		public static IApplicationBuilder UseOperationsApi(this IApplicationBuilder app)
		{
			app.UseServerTimingReporter();
			app.UseRequestProfiling();
			app.UseOperationsEndpoints();
			return app;
		}

		internal static IApplicationBuilder UseRequestProfiling(this IApplicationBuilder app)
		{
			return app.Use(async (context, next) =>
			{
				var options = context.RequestServices.GetService<IOptions<OperationsApiOptions>>();

				if (options?.Value != null && options.Value.EnableRequestProfiling &&
				    !options.Value.MetricsOptions.EnableServerTiming)
				{
					var sw = StopwatchPool.Pool.Get();

					context.Response.OnStarting(() =>
					{
						var duration = sw.Elapsed;
						StopwatchPool.Pool.Return(sw);
						var header = options.Value.RequestProfilingHeader ?? HeaderNames.ServerTiming;
						context.Response.Headers.Add(header, $"roundtrip;dur={duration.TotalMilliseconds};desc=\"*\"");
						return Task.CompletedTask;
					});
				}

				await next();
			});
		}

		internal static IApplicationBuilder UseOperationsEndpoints(this IApplicationBuilder app)
		{
			return app.Use(async (context, next) =>
			{
				var options = context.RequestServices.GetRequiredService<IOptions<OperationsApiOptions>>();

				if (options.Value != null &&
				    options.Value.EnableEnvironmentEndpoint &&
				    !string.IsNullOrWhiteSpace(options.Value.EnvironmentEndpointPath) &&
				    context.Request.Path.Value.StartsWith(
					    options.Value.RootPath + options.Value.EnvironmentEndpointPath))
				{
					await OperationsHandlers.GetEnvironmentHandler(app, context);
					return;
				}

				if (options.Value != null &&
				    options.Value.EnableRouteDebugging &&
				    !string.IsNullOrWhiteSpace(options.Value.RouteDebuggingPath) &&
				    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.RouteDebuggingPath))
				{
					await OperationsHandlers.GetRoutesDebugHandler(context, app);
					return;
				}

				if (options.Value != null &&
				    options.Value.EnableOptionsDebugging &&
				    !string.IsNullOrWhiteSpace(options.Value.OptionsDebuggingPath) &&
				    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.OptionsDebuggingPath))
				{
					await OperationsHandlers.GetOptionsDebugHandler(context, app);
					return;
				}

				if (options.Value != null &&
				    options.Value.EnableServicesDebugging &&
				    !string.IsNullOrWhiteSpace(options.Value.ServicesDebuggingPath) &&
				    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.ServicesDebuggingPath))
				{
					await OperationsHandlers.GetServicesDebugHandler(context, app);
					return;
				}

				if (options.Value != null &&
				    options.Value.EnableHostedServicesDebugging &&
				    !string.IsNullOrWhiteSpace(options.Value.HostedServicesDebuggingPath) &&
				    context.Request.Path.Value.StartsWith(options.Value.RootPath +
				                                          options.Value.HostedServicesDebuggingPath))
				{
					await OperationsHandlers.GetHostedServicesDebugHandler(context, app);
					return;
				}

				if (options.Value != null &&
				    options.Value.EnableMetricsEndpoint &&
				    !string.IsNullOrWhiteSpace(options.Value.MetricsEndpointPath) &&
				    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.MetricsEndpointPath))
				{
					await OperationsHandlers.GetMetricsHandler(context, options.Value, app);
					return;
				}

				if (options.Value != null && options.Value.EnableHealthChecksEndpoints)
				{
					if (!string.IsNullOrWhiteSpace(options.Value.HealthCheckLivePath) &&
					    context.Request.Path.Value.StartsWith(
						    options.Value.RootPath + options.Value.HealthCheckLivePath))
					{
						await OperationsHandlers.GetHealthChecksHandler(context, r => false, app);
						return;
					}

					if (!string.IsNullOrWhiteSpace(options.Value.HealthChecksPath) &&
					    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.HealthChecksPath))
					{
						context.Request.Query.TryGetValue("tags", out var tags);
						await OperationsHandlers.GetHealthChecksHandler(context, r => r.Tags.IsSupersetOf(tags), app);
						return;
					}
				}

				if (options.Value != null &&
				    options.Value.EnableFeatureDebugging &&
				    !string.IsNullOrWhiteSpace(options.Value.FeatureDebuggingPath) &&
				    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.FeatureDebuggingPath))
				{
					await OperationsHandlers.GetFeaturesDebugHandler(context, app);
					return;
				}

				if (options.Value != null &&
				    options.Value.EnableCacheDebugging &&
				    !string.IsNullOrWhiteSpace(options.Value.CacheDebuggingPath) &&
				    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.CacheDebuggingPath))
				{
					await OperationsHandlers.GetCacheDebugHandler(context, app);
					return;
				}

				if (options.Value != null &&
				    options.Value.EnableEnvironmentEndpoint &&
				    !string.IsNullOrWhiteSpace(options.Value.EnvironmentEndpointPath) &&
				    context.Request.Path.Value.StartsWith(
					    options.Value.RootPath + options.Value.EnvironmentEndpointPath))
				{
					await OperationsHandlers.GetEnvironmentHandler(app, context);
					return;
				}

				await next();
			});
		}

		public static IApplicationBuilder UseConfigurationApi(this IApplicationBuilder app)
		{
#if NETCOREAPP2_2
			app.UseMvc();
#else
			app.UseRouting();
			app.UseEndpoints(endpoints => endpoints.MapControllers());
#endif
			return app;
		}

		public static IApplicationBuilder UseMetaApi(this IApplicationBuilder app)
		{
#if NETCOREAPP2_2
			app.UseMvc();
#else
			app.UseRouting();
			app.UseEndpoints(endpoints => endpoints.MapControllers());
#endif
			return app;
		}
	}
}