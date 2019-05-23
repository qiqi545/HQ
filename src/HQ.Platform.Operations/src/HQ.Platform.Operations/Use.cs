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

using System.IO;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Extensions.Metrics.Reporters.ServerTiming;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Operations
{
    public static class Use
    {
        public static IApplicationBuilder UseOperationsApi(this IApplicationBuilder app)
        {
            app.UseExceptionHandler();
            app.UseServerTimingReporter();
            app.UseRequestProfiling();
            app.UseOperationsEndpoints();
            return app;
        }

        internal static IApplicationBuilder UseExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseExceptionHandler(x =>
            {
                x.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/html";

                    await context.Response.WriteAsync("<html lang=\"en\"><body>\r\n");
                    await context.Response.WriteAsync("ERROR!<br><br>\r\n");

                    var exceptionHandlerPathFeature =
                        context.Features.Get<IExceptionHandlerPathFeature>();

                    // Use exceptionHandlerPathFeature to process the exception (for example, 
                    // logging), but do NOT expose sensitive error information directly to 
                    // the client.

                    if (exceptionHandlerPathFeature?.Error is FileNotFoundException)
                    {
                        await context.Response.WriteAsync("File error thrown!<br><br>\r\n");
                    }

                    await context.Response.WriteAsync("<a href=\"/\">Home</a><br>\r\n");
                    await context.Response.WriteAsync("</body></html>\r\n");
                    await context.Response.WriteAsync(new string(' ', 512)); // IE padding
                });
            });
        }

        internal static IApplicationBuilder UseRequestProfiling(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var options = context.RequestServices.GetService<IOptions<OperationsApiOptions>>();

                if (options?.Value != null && options.Value.EnableRequestProfiling && !options.Value.MetricsOptions.EnableServerTiming)
                {
                    var sw = StopwatchPool.Pool.Get();

                    context.Response.OnStarting(() =>
                    {
                        var duration = sw.Elapsed;
                        StopwatchPool.Pool.Return(sw);
                        var header = options.Value.RequestProfilingHeader ?? Constants.HttpHeaders.ServerTiming;
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
                    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.EnvironmentEndpointPath))
                {
                    await OperationsEndpoints.GetEnvironmentHandler(app, context);
                    return;
                }

                if (options.Value != null &&
                    options.Value.EnableRouteDebugging &&
                    !string.IsNullOrWhiteSpace(options.Value.RouteDebuggingPath) &&
                    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.RouteDebuggingPath))
                {
                    await OperationsEndpoints.GetRoutesDebugHandler(context, app);
                    return;
                }

                if (options.Value != null &&
                    options.Value.EnableOptionsDebugging &&
                    !string.IsNullOrWhiteSpace(options.Value.OptionsDebuggingPath) && 
                    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.OptionsDebuggingPath))
                {
                    await OperationsEndpoints.GetOptionsDebugHandler(context, app);
                    return;
                }

                if (options.Value != null &&
                    options.Value.EnableServicesDebugging &&
                    !string.IsNullOrWhiteSpace(options.Value.ServicesDebuggingPath) &&
                    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.ServicesDebuggingPath))
                {
                    await OperationsEndpoints.GetServicesDebugHandler(context, app);
                    return;
                }

                if (options.Value != null &&
                    options.Value.EnableMetricsEndpoint &&
                    !string.IsNullOrWhiteSpace(options.Value.MetricsEndpointPath) &&
                    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.MetricsEndpointPath))
                {
                    await OperationsEndpoints.GetMetricsHandler(context, options.Value, app);
                    return;
                }

                if (options.Value != null && options.Value.EnableHealthChecksEndpoints)
                {
                    if (!string.IsNullOrWhiteSpace(options.Value.HealthCheckLivePath) && context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.HealthCheckLivePath))
                    {
                        await OperationsEndpoints.GetHealthChecksHandler(context, r => false, app);
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(options.Value.HealthChecksPath) && context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.HealthChecksPath))
                    {
                        context.Request.Query.TryGetValue("tags", out var tags);
                        await OperationsEndpoints.GetHealthChecksHandler(context, r => r.Tags.IsSupersetOf(tags), app);
                        return;
                    }
                }

                if (options.Value != null &&
                    options.Value.EnableFeatureDebugging &&
                    !string.IsNullOrWhiteSpace(options.Value.FeatureDebuggingPath) &&
                    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.FeatureDebuggingPath))
                {
                    await OperationsEndpoints.GetFeaturesDebugHandler(context, app);
                    return;
                }

                if (options.Value != null &&
                    options.Value.EnableCacheDebugging &&
                    !string.IsNullOrWhiteSpace(options.Value.CacheDebuggingPath) &&
                    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.CacheDebuggingPath))
                {
                    await OperationsEndpoints.GetCacheDebugHandler(context, app);
                    return;
                }

                await next();
            });
        }
        
        
    }
}
