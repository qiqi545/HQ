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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Extensions.Caching;
using HQ.Extensions.Metrics;
using HQ.Extensions.Metrics.Internal;
using HQ.Platform.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Operations
{
    internal static class OperationsHandlers
    {
        public static async Task GetEnvironmentHandler(IApplicationBuilder app, HttpContext context)
        {
            string GetPlatform()
            {
                return
                    RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" :
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" :
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "OSX" : "Unknown";
            }

            // See: https://github.com/dotnet/BenchmarkDotNet/issues/448#issuecomment-308424100
            string GetNetCoreVersion()
            {
                var assembly = typeof(GCSettings).Assembly;
                var assemblyPath = assembly.CodeBase.Split(new[] {'/', '\\'}, StringSplitOptions.RemoveEmptyEntries);
                var netCoreAppIndex = Array.IndexOf(assemblyPath, "Microsoft.NETCore.App");
                return netCoreAppIndex > 0 && netCoreAppIndex < assemblyPath.Length - 2
                    ? assemblyPath[netCoreAppIndex + 1]
                    : null;
            }

            var hosting = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            var config = context.RequestServices.GetRequiredService<IConfiguration>();

            var process = Process.GetCurrentProcess();
            var hostName = Dns.GetHostName();
            var hostEntry = Dns.GetHostEntry(hostName);

            IDictionary<string, object> configuration = new ExpandoObject();
            foreach (var entry in config.AsEnumerable())
            {
                configuration.Add(entry.Key, entry.Value);
            }

            var env = new
            {
                Dns =
                    new
                    {
                        HostName = hostName,
                        HostEntry = new
                        {
                            hostEntry.Aliases, Addresses = hostEntry.AddressList.Select(x => x.ToString())
                        }
                    },
                OperatingSystem =
                    new
                    {
                        Platform = GetPlatform(),
                        Description = RuntimeInformation.OSDescription,
                        Architecture = RuntimeInformation.OSArchitecture,
                        Version = Environment.OSVersion,
                        Is64Bit = Environment.Is64BitOperatingSystem
                    },
                Process = new
                {
                    process.Id,
                    process.ProcessName,
                    process.MachineName,
                    Arguments = Environment.GetCommandLineArgs(),
                    Architecture = RuntimeInformation.ProcessArchitecture,
                    MaxWorkingSet = process.MaxWorkingSet.ToInt64(),
                    MinWorkingSet = process.MinWorkingSet.ToInt64(),
                    process.PagedMemorySize64,
                    process.PeakWorkingSet64,
                    process.PrivateMemorySize64,
                    process.StartTime,
                    Is64Bit = Environment.Is64BitProcess
                },
                Environment =
                    new
                    {
                        hosting.EnvironmentName,
                        hosting.ApplicationName,
                        hosting.ContentRootPath,
                        hosting.WebRootPath,
                        Environment.CurrentDirectory,
                        Environment.CurrentManagedThreadId
                    },
                Framework = new
                {
                    Version = $"{RuntimeInformation.FrameworkDescription}",
                    NetCoreVersion = GetNetCoreVersion(),
                    ClrVersion = Environment.Version.ToString()
                },
                Configuration = configuration
            };

            await app.WriteResultAsJson(context, env);
        }

        public static async Task GetHealthChecksHandler(HttpContext context, Func<HealthCheckRegistration, bool> filter, IApplicationBuilder app)
        {
            var options = context.RequestServices.GetRequiredService<IOptionsMonitor<HealthCheckOptions>>();
            var service = context.RequestServices.GetRequiredService<HealthCheckService>();
            var report = await service.CheckHealthAsync(filter ?? options.CurrentValue.Predicate, context.RequestAborted);
            if (!options.CurrentValue.ResultStatusCodes.TryGetValue(report.Status, out var num))
                throw new InvalidOperationException($"No status code mapping found for {"HealthStatus" as object} value: {report.Status as object}.HealthCheckOptions.ResultStatusCodes must contain an entry for {report.Status as object}.");
            
            context.Response.StatusCode = num;

            if (!options.CurrentValue.AllowCachingResponses)
            {
                var headers = context.Response.Headers;
                headers["Cache-Control"] = "no-store, no-cache";
                headers["Pragma"] = "no-cache";
                headers["Expires"] = "Thu, 01 Jan 1970 00:00:00 GMT";
            }

            await app.WriteResultAsJson(context, report);
        }

        public static async Task GetMetricsHandler(HttpContext context, OperationsApiOptions options, IApplicationBuilder app)
        {
            var registry = context.RequestServices.GetRequiredService<IMetricsRegistry>();
            var timeout = TimeSpan.FromSeconds(options.MetricsOptions.SampleTimeoutSeconds);
            var cancel = new CancellationTokenSource(timeout);
            var samples = await Task.Run(() => registry.SelectMany(x => x.GetSample()).ToImmutableDictionary(), cancel.Token);
            var json = JsonSampleSerializer.Serialize(samples);

            await app.WriteResultAsJson(context, json);
        }

        public static Task GetRoutesDebugHandler(HttpContext context, IApplicationBuilder app)
        {
            var provider = context.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();

            var map = provider.ActionDescriptors.Items.Select(Map);

            object Map(ActionDescriptor descriptor)
            {
                var controller = descriptor.RouteValues["Controller"];
                var action = descriptor.RouteValues["Action"];
                var constraints = descriptor.ActionConstraints;
                var filters = descriptor.FilterDescriptors.OrderBy(x => x.Order)
                    .ThenBy(x => x.Scope)
                    .Select(x => x.Filter.GetType().Name);

                return new
                {
                    descriptor.Id,
                    Path = $"{controller}/{action}",
                    Action = action,
                    descriptor.DisplayName,
                    descriptor.AttributeRouteInfo?.Template,
                    descriptor.AttributeRouteInfo?.Name,
                    Filters = filters,
                    Constraints = constraints
                };
            }

            return app.WriteResultAsJson(context, map);
        }

        public static Task GetServicesDebugHandler(HttpContext context, IApplicationBuilder app)
        {
            return app.WriteResultAsJson(context, OperationsMethods.GetServicesReport(context.RequestServices));
        }

        public static Task GetOptionsDebugHandler(HttpContext context, IApplicationBuilder app)
        {
            return app.WriteResultAsJson(context, OperationsMethods.GetOptionsReport(app.ApplicationServices));
        }

        public static Task GetFeaturesDebugHandler(HttpContext context, IApplicationBuilder app)
        {
            var registered = new Dictionary<string, bool>();
            var unregistered = new HashSet<string>();
            var indeterminate = new HashSet<string>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var featureType in assembly.GetTypes())
                {
                    if (!featureType.IsSubclassOf(typeof(FeatureToggle)))
                        continue;

                    Type optionsWrapperType;

                    try
                    {
                        optionsWrapperType = typeof(IOptions<>).MakeGenericType(featureType);
                    }
                    catch
                    {
                        indeterminate.Add(featureType.Name);
                        continue;
                    }

                    var instance = context.RequestServices.GetService(optionsWrapperType) ??
                                   context.RequestServices.GetService(featureType);

                    if (instance == null)
                    {
                        unregistered.Add(featureType.Name);
                        continue;
                    }

                    var serviceType = instance.GetType();
                    if (optionsWrapperType.IsAssignableFrom(serviceType))
                    {
                        var property = optionsWrapperType.GetProperty(nameof(IOptions<object>.Value));
                        if (!(property?.GetValue(instance) is FeatureToggle feature))
                        {
                            indeterminate.Add(featureType.Name);
                            continue;
                        }

                        registered[featureType.Name] = feature.Enabled;
                        continue;
                    }

                    if (serviceType == featureType)
                    {
                        if (instance is FeatureToggle feature)
                        {
                            registered[featureType.Name] = feature.Enabled;
                        }
                    }
                }
            }

            return app.WriteResultAsJson(context, new
            {
                Registered = registered,
                Unregistered = unregistered,
                Indeterminate = indeterminate
            });
        }

        public static Task GetCacheDebugHandler(HttpContext context, IApplicationBuilder app)
        {
            // TODO Caches - Formal (IDistributedCache, IOptionsSnapshot, etc.)
            // TODO Caches - Opaque (Dictionary, etc.)

            var totalCacheKeys = 0L;
            var totalCacheMemory = 0L;

            var managed = new List<object>();
            var unmanaged = new List<object>();

            foreach (var cache in app.ApplicationServices.GetServices<ICache>())
            {
                if (cache is ICacheManager manager)
                {
                    totalCacheMemory += manager.SizeBytes;
                    totalCacheKeys += manager.Count;

                    managed.Add(new
                    {
                        Type = manager.GetType().Name,
                        Count = manager.Count,
                        Size = manager.SizeBytes,
                        SizeLimit = manager.SizeLimitBytes
                    });

                    continue;
                }

                unmanaged.Add(new
                {
                    Type = cache.GetType().Name
                });
            }

            foreach (var cache in app.ApplicationServices.GetServices<IHttpCache>())
            {
                if (cache is ICacheManager manager)
                {
                    totalCacheMemory += manager.SizeBytes;
                    totalCacheKeys += manager.Count;

                    managed.Add(new
                    {
                        Type = manager.GetType().Name,
                        Count = manager.Count,
                        Size = manager.SizeBytes,
                        SizeLimit = manager.SizeLimitBytes
                    });

                    continue;
                }

                unmanaged.Add(new
                {
                    Type = cache.GetType().Name
                });
            }

            return app.WriteResultAsJson(context, new
            {
                Managed = managed,
                TotalMemory = GC.GetTotalMemory(false),
                TotalCacheKeys = totalCacheKeys,
                TotalCacheMemory = totalCacheMemory,
                Unmanaged = unmanaged
            });
        }
    }
}
