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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Extensions.Metrics;
using HQ.Extensions.Metrics.Internal;
using HQ.Extensions.Metrics.Reporters.ServerTiming;
using HQ.Platform.Api.Models;
using HQ.Platform.Api.Serialization;
using HQ.Platform.Operations.Configuration;
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
using Newtonsoft.Json;

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
            Task GetRoutesDebugHandler(HttpContext context)
            {
                var provider = context.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();

                var map = provider.ActionDescriptors.Items.Select(Map);

                return WriteResultAsJson(app, context, map, context.RequestAborted);
            }

            object Map(ActionDescriptor descriptor)
            {
                var controller = descriptor.RouteValues["Controller"];
                var action = descriptor.RouteValues["Action"];
                var constraints = descriptor.ActionConstraints;
                var filters = descriptor.FilterDescriptors.OrderBy(x => x.Order).ThenBy(x => x.Scope)
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

            Task GetOptionsDebugHandler(HttpContext context)
            {
                var optionTypes = typeof(IOptions<>).GetImplementationsOfOpenGeneric();

                var model = optionTypes.GroupBy(x => x.Name).Select(x =>
                {
                    // i.e., IOptions, IOptionsSnapshot, IOptionsMonitor, etc.
                    var scope = x.Key.Substring(0, x.Key.Length - 2 /* `1 */);

                    var values = x.Distinct().Select(t =>
                    {
                        var valid = TryBindOptions(context.RequestServices, t, out var options);

                        return new
                        {
                            Type = GetOptionsTypeName(t),
                            IsValid = valid,
                            Value = options
                        };
                    }).OrderByDescending(v => !v.IsValid).ThenBy(v => v.Type).ToList();

                    return new
                    {
                        Scope = scope,
                        HasErrors = values.Any(v => !v.IsValid),
                        Values = values
                    };
                }).ToList();

                return WriteResultAsJson(app, context, model, context.RequestAborted);
            }

            Task GetServicesDebugHandler(HttpContext context)
            {
                var services = context.RequestServices.GetRequiredService<IServiceCollection>();
                var missingRegistrations = new HashSet<string>();

                var manifest = services.Select(x =>
                {
                    var serviceTypeName = x.ServiceType.Name;
                    var implementationTypeName = x.ImplementationType?.Name;
                    var implementationInstanceName = x.ImplementationInstance?.GetType().Name;

                    string implementationFactoryTypeName = null;
                    if (x.ImplementationFactory != null)
                    {
                        try
                        {
                            var result = x.ImplementationFactory.Invoke(context.RequestServices);
                            if (result != null)
                            {
                                implementationFactoryTypeName = result.GetType().Name;
                            }
                        }
                        catch(InvalidOperationException ex)
                        {
                            if (ex.Source == "Microsoft.Extensions.DependencyInjection.Abstractions")
                            {
                                var match = Regex.Match(ex.Message, "No service for type '([\\w.]*)'",
                                    RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
                                
                                if(match.Success)
                                {
                                    var typeName = match.Groups[1];
                                    missingRegistrations.Add(typeName.Value);
                                }
                            }
                            else
                            {
                                Trace.TraceError($"{ex.ToString()}");
                            }
                        }
                    }

                    return new
                    {
                        x.Lifetime,
                        ImplementationType = implementationTypeName,
                        ImplementationInstance = implementationInstanceName,
                        ImplementationFactory = implementationFactoryTypeName,
                        ServiceType = serviceTypeName
                    };
                }).ToList();

                return WriteResultAsJson(app, context, new
                {
                    MissingRegistrations = missingRegistrations,
                    Manifest = manifest
                }, context.RequestAborted);
            }

            async Task GetMetricsHandler(HttpContext context, OperationsApiOptions options)
            {
                var registry = context.RequestServices.GetRequiredService<IMetricsRegistry>();
                var timeout = TimeSpan.FromSeconds(options.MetricsOptions.SampleTimeoutSeconds);
                var cancel = new CancellationTokenSource(timeout);
                var samples = await Task.Run(()=> registry.SelectMany(x => x.GetSample()).ToImmutableDictionary(), cancel.Token);
                var json = JsonSampleSerializer.Serialize(samples);
                await WriteResultAsJson(app, context, json, context.RequestAborted);
            }

            async Task GetHealthChecksHandler(HttpContext context, Func<HealthCheckRegistration, bool> filter)
            {
                var options = context.RequestServices.GetRequiredService<IOptionsMonitor<HealthCheckOptions>>();
                var service = context.RequestServices.GetRequiredService<HealthCheckService>();

                var report = await service.CheckHealthAsync(filter ?? options.CurrentValue.Predicate, context.RequestAborted);

                if (!options.CurrentValue.ResultStatusCodes.TryGetValue(report.Status, out var num))
                    throw new InvalidOperationException($"No status code mapping found for {"HealthStatus" as object} value: {(object) report.Status}." + "HealthCheckOptions.ResultStatusCodes must contain" + string.Format("an entry for {0}.", (object)report.Status));
                
                context.Response.StatusCode = num;

                if (!options.CurrentValue.AllowCachingResponses)
                {
                    var headers = context.Response.Headers;
                    headers["Cache-Control"] = "no-store, no-cache";
                    headers["Pragma"] = "no-cache";
                    headers["Expires"] = "Thu, 01 Jan 1970 00:00:00 GMT";
                }

                await WriteResultAsJson(app, context, report, context.RequestAborted);
            }

            return app.Use(async (context, next) =>
            {
                var options = context.RequestServices.GetRequiredService<IOptions<OperationsApiOptions>>();

                if (options.Value != null &&
                    options.Value.EnableEnvironmentEndpoint &&
                    !string.IsNullOrWhiteSpace(options.Value.EnvironmentEndpointPath) &&
                    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.EnvironmentEndpointPath))
                {
                    await GetEnvironmentHandler(app, context);
                    return;
                }

                if (options.Value != null &&
                    options.Value.EnableRouteDebugging &&
                    !string.IsNullOrWhiteSpace(options.Value.RouteDebuggingPath) &&
                    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.RouteDebuggingPath))
                {
                    await GetRoutesDebugHandler(context);
                    return;
                }

                if (options.Value != null &&
                    options.Value.EnableOptionsDebugging &&
                    !string.IsNullOrWhiteSpace(options.Value.OptionsDebuggingPath) && 
                    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.OptionsDebuggingPath))
                {
                    await GetOptionsDebugHandler(context);
                    return;
                }

                if (options.Value != null &&
                    options.Value.EnableServicesDebugging &&
                    !string.IsNullOrWhiteSpace(options.Value.ServicesDebuggingPath) &&
                    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.ServicesDebuggingPath))
                {
                    await GetServicesDebugHandler(context);
                    return;
                }

                if (options.Value != null &&
                    options.Value.EnableMetricsEndpoint &&
                    !string.IsNullOrWhiteSpace(options.Value.MetricsEndpointPath) &&
                    context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.MetricsEndpointPath))
                {
                    await GetMetricsHandler(context, options.Value);
                    return;
                }

                if (options.Value != null && options.Value.EnableHealthChecksEndpoints)
                {
                    if (!string.IsNullOrWhiteSpace(options.Value.HealthCheckLivePath) && context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.HealthCheckLivePath))
                    {
                        await GetHealthChecksHandler(context, r => false);
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(options.Value.HealthChecksPath) && context.Request.Path.Value.StartsWith(options.Value.RootPath + options.Value.HealthChecksPath))
                    {
                        context.Request.Query.TryGetValue("tags", out var tags);
                        await GetHealthChecksHandler(context, r => r.Tags.IsSupersetOf(tags));
                        return;
                    }
                }

                await next();
            });
        }

        private static bool TryBindOptions(this IServiceProvider serviceProvider, Type optionsWrapperType, out object options)
        {
            // IOptions<T>
            var resolved = optionsWrapperType.GetGenericArguments()[0];
            while (resolved != null && resolved.IsGenericType)
            {
                resolved = resolved.IsGenericTypeDefinition
                    ? resolved.MakeGenericType(optionsWrapperType.GetGenericArguments())    // IOptions<TService<T1,...TN>>
                    : resolved.BaseType;                                                    // HubOptions<THub> -> HubOptions
            }
            resolved = typeof(IOptions<>).MakeGenericType(resolved);

            try
            {
                var instance = serviceProvider.GetService(resolved);
                var property = resolved.GetProperty(nameof(IOptions<object>.Value));
                options = property?.GetValue(instance);
                return options != null;
            }
            catch (Exception e)
            {
                options = new
                {
                    Type = GetOptionsTypeName(optionsWrapperType),
                    ErrorType = e.GetType().Name,
                    Message = e.Message,
                    StackTrace = e.StackTrace
                };
                return false;
            }
        }

        private static string GetOptionsTypeName(this Type t)
        {
            return t.IsGenericParameter && t.DeclaringMethod != null && t.DeclaringMethod.IsGenericMethod
                ?
                t.DeclaringMethod.IsGenericMethod ? t.DeclaringMethod.GetGenericArguments()[0].Name :
                t.DeclaringMethod.Name
                : t.IsGenericType
                    ? t.GetGenericArguments()[0].Name
                    : t.Name;
        }
        
        private static async Task GetEnvironmentHandler(IApplicationBuilder app, HttpContext context)
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
                Dns = new
                {
                    HostName = hostName,
                    HostEntry = new
                    {
                        hostEntry.Aliases,
                        Addresses = hostEntry.AddressList.Select(x => x.ToString())
                    }
                },
                OperatingSystem = new
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
                Environment = new
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

            await WriteResultAsJson(app, context, env, context.RequestAborted);
        }

        private static async Task WriteResultAsJson(IApplicationBuilder app, HttpContext context, object instance, CancellationToken cancellationToken)
        {
            context.Response.Headers.Add(Constants.HttpHeaders.ContentType, Constants.MediaTypes.Json);
            await context.Response.WriteAsync(SerializeObject(app, context, instance), cancellationToken: cancellationToken);
        }

        private static string SerializeObject(IApplicationBuilder app, HttpContext context, object instance)
        {
            var serializerSettings = app.ApplicationServices.GetService<JsonSerializerSettings>();
            if (serializerSettings != null)
            {
                if (context.Items[Constants.ContextKeys.JsonMultiCase] is ITextTransform transform)
                {
                    serializerSettings.ContractResolver =
                        new JsonContractResolver(transform, JsonProcessingDirection.Output);
                }
                else
                {
                    serializerSettings = JsonConvert.DefaultSettings();
                }
            }

            var json = serializerSettings != null
                ? JsonConvert.SerializeObject(instance, serializerSettings)
                : JsonConvert.SerializeObject(instance);
            return json;
        }
    }
}
