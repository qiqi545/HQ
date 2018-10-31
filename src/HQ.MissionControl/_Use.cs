using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HQ.Common.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using HQ.Common;
using HQ.MissionControl.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace HQ.MissionControl
{
	public static class Use
	{
		public static IApplicationBuilder UseDevOpsApi(this IApplicationBuilder app, Action<IRouteBuilder> configureRoutes = null)
		{
			Bootstrap.EnsureInitialized();

			app.UseRequestProfiling();
			app.UseEnvironmentEndpoint();

			var options = app.ApplicationServices.GetService<IOptions<DevOpsOptions>>();
			var enabled = options.Value.EnableRouteDebugging;
			if (options?.Value != null && enabled)
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
							Task GetRoutes(HttpContext c)
							{
								var provider = c.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
								var objects = provider.ActionDescriptors.Items.Select(Map);
								var json = JsonConvert.SerializeObject(objects);
								c.Response.Headers.Add(HttpHeaders.ContentType, MediaTypes.Json);
								return c.Response.WriteAsync(json);
							}

							routes.MapGet(options.Value.RouteDebuggingPath ?? "routes", GetRoutes);
						}

						configureRoutes?.Invoke(routes);
					});
				});
			}

			return app;
		}

		internal static IApplicationBuilder UseRequestProfiling(this IApplicationBuilder app)
		{
			return app.Use(async (context, next) =>
			{
				var options = context.RequestServices.GetService<IOptions<DevOpsOptions>>();
				if (options?.Value != null && options.Value.EnableRequestProfiling)
				{
					StopwatchPool.Scoped(async sw =>
					{
						context.Response.OnStarting(() =>
						{
							var header = options.Value.RequestProfilingHeader ?? HqHeaders.ExecutionTimeMs;
							context.Response.Headers.Add(header, $"{sw.Elapsed.TotalMilliseconds}");
							return Task.CompletedTask;
						});

						await next();
					});
				}
				else
				{
					await next();
				}
			});
		}

		internal static IApplicationBuilder UseEnvironmentEndpoint(this IApplicationBuilder app)
		{
			string GetPlatform()
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
					return "Linux";
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					return "Windows";
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
					return "OSX";
				return "Unknown";
			}

			// See: https://github.com/dotnet/BenchmarkDotNet/issues/448#issuecomment-308424100
			string GetNetCoreVersion()
			{
				var assembly = typeof(GCSettings).Assembly;
				var assemblyPath = assembly.CodeBase.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
				var netCoreAppIndex = Array.IndexOf(assemblyPath, "Microsoft.NETCore.App");
				return netCoreAppIndex > 0 && netCoreAppIndex < assemblyPath.Length - 2
					? assemblyPath[netCoreAppIndex + 1]
					: null;
			}

			return app.Use(async (context, next) =>
			{
				var options = context.RequestServices.GetRequiredService<IOptions<DevOpsOptions>>();
				var template = options.Value.RootPath + options.Value.EnvironmentEndpointPath;

				if (options?.Value != null && 
				    options.Value.EnableEnvironmentEndpoint && 
				    context.Request.Path.Value.StartsWith(template))
				{
					var hosting = context.RequestServices.GetRequiredService<IHostingEnvironment>();
					var config = context.RequestServices.GetRequiredService<IConfiguration>();

					var process = Process.GetCurrentProcess();
					var hostName = Dns.GetHostName();
					var hostEntry = Dns.GetHostEntry(hostName);

					IDictionary<string, object> configuration = new ExpandoObject();
					foreach(var entry in config.AsEnumerable())
						configuration.Add(entry.Key, entry.Value);

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
							Is64Bit = Environment.Is64BitOperatingSystem,
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
							Is64Bit = Environment.Is64BitProcess,
						},
						Environment = new
						{
							hosting.EnvironmentName,
							hosting.ApplicationName,
							hosting.ContentRootPath,
							hosting.WebRootPath,
							Environment.CurrentDirectory,
							Environment.CurrentManagedThreadId,
						},
						Framework = new
						{
							Version = $"{RuntimeInformation.FrameworkDescription}",
							NetCoreVersion = GetNetCoreVersion(),
							ClrVersion = Environment.Version.ToString(),
						},
						Configuration = configuration,
					};

					context.Response.Headers.Add(HttpHeaders.ContentType, MediaTypes.Json);
					var serializerSettings = app.ApplicationServices.GetService<JsonSerializerSettings>();

					var json = serializerSettings != null
						? JsonConvert.SerializeObject(env, serializerSettings)
						: JsonConvert.SerializeObject(env);

					await context.Response.WriteAsync(json);
					return;
				}
				await next();
			});
		}
	}
}