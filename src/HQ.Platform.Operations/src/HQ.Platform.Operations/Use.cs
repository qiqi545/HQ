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
using System.Reflection;
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
            app.UseRequestProfiling();
            app.UseEnvironmentEndpoint();
            return app;
        }

        internal static IApplicationBuilder UseRequestProfiling(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var options = context.RequestServices.GetService<IOptions<DevOpsApiOptions>>();
                if (options?.Value != null && options.Value.EnableRequestProfiling)
                {
                    var sw = StopwatchPool.Pool.Get();

                    context.Response.OnStarting(() =>
                    {
                        var elapsed = sw.Elapsed;
                        StopwatchPool.Pool.Return(sw);
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

            Task GetRoutesDebugHandler(HttpContext context)
            {
                return WriteResultAsJson(app, context,
                    context.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>()
                        .ActionDescriptors.Items.Select(Map));
            }

            Task GetOptionsDebugHandler(HttpContext context)
            {
                var optionTypes = typeof(IOptions<>).GetImplementationInstancesOfOpenGeneric();

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

                return WriteResultAsJson(app, context, model);
            }

            return app.Use(async (context, next) =>
            {
                var options = context.RequestServices.GetRequiredService<IOptions<DevOpsApiOptions>>();

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

        private static IEnumerable<Type> GetImplementationInstancesOfOpenGeneric(this Type openGenericType)
        {
            return GetInstancesOfOpenGeneric(openGenericType, AppDomain.CurrentDomain.GetAssemblies());
        }

        private static IEnumerable<Type> GetInstancesOfOpenGeneric(this Type openGenericType, IEnumerable<Assembly> assemblies)
        {
            if (!openGenericType.IsGenericType)
                throw new ArgumentException("provided type is not an open generic type", nameof(openGenericType));

            foreach (var assembly in assemblies)
            {
                var members = assembly.GetTypes().SelectMany(x => x.GetMembers());
                
                foreach (var member in members)
                {
                    if (!(member is MethodBase ctorOrMethod))
                        continue;

                    foreach (var parameter in ctorOrMethod.GetParameters())
                    {
                        if (parameter.ParameterType.ImplementsGeneric(openGenericType))
                        {
                            yield return parameter.ParameterType;
                        }
                    }
                }
            }
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

            await WriteResultAsJson(app, context, env);
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
