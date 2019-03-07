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
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Platform.Api.Models;
using HQ.Platform.Api.Serialization;
using HQ.Platform.Operations.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HQ.Platform.Operations
{
    public static class Use
    {
        public static IApplicationBuilder UseDevOpsApi(this IApplicationBuilder app)
        {
            Bootstrap.EnsureInitialized();

            app.UseRequestProfiling();
            app.UseEnvironmentEndpoint();

            var options = app.ApplicationServices.GetService<IOptions<DevOpsApiOptions>>();
            var enabled = options.Value.EnableRouteDebugging;
            if (options.Value != null && enabled)
            {
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

                app.Map(options.Value.RootPath ?? string.Empty, x =>
                {
                    x.UseMvc(routes =>
                    {
                        if (options.Value.EnableRouteDebugging)
                        {
                            Task GetRoutesHandler(HttpContext context)
                            {
                                return WriteResultAsJson(app, context,
                                    context.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>()
                                        .ActionDescriptors.Items.Select(Map));
                            }

                            routes.MapGet(options.Value.RouteDebuggingPath ?? "/routes", GetRoutesHandler);
                        }
                    });
                });
            }

            return app;
        }

        internal static IApplicationBuilder UseRequestProfiling(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var options = context.RequestServices.GetService<IOptions<DevOpsApiOptions>>();
                if (options?.Value != null && options.Value.EnableRequestProfiling)
                {
                    var sw = PooledStopwatch.StartInstance();

                    context.Response.OnStarting(() =>
                    {
                        var elapsed = sw.Elapsed;
                        sw.Free();
                        var header = options.Value.RequestProfilingHeader ?? Constants.HttpHeaders.ServerTiming;
                        context.Response.Headers.Add(header, $"roundtrip;dur={elapsed.TotalMilliseconds};desc=\"*\"");
                        return Task.CompletedTask;
                    });
                }

                await next();
            });
        }

        internal static IApplicationBuilder UseEnvironmentEndpoint(this IApplicationBuilder app)
        {
            string GetPlatform()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return "Linux";
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return "Windows";
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return "OSX";
                }

                return "Unknown";
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

            return app.Use(async (context, next) =>
            {
                var options = context.RequestServices.GetRequiredService<IOptions<DevOpsApiOptions>>();
                var template = options.Value.RootPath + options.Value.EnvironmentEndpointPath;

                if (options.Value != null &&
                    options.Value.EnableEnvironmentEndpoint &&
                    context.Request.Path.Value.StartsWith(template))
                {
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

                    await WriteResultAsJson(app, context, env);
                    return;
                }

                await next();
            });
        }

        private static async Task WriteResultAsJson(IApplicationBuilder app, HttpContext context, object env)
        {
            context.Response.Headers.Add(Constants.HttpHeaders.ContentType, Constants.MediaTypes.Json);
            await context.Response.WriteAsync(SerializeObject(app, context, env));
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
