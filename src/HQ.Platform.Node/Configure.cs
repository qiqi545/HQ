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
using System.Diagnostics;
using System.Reflection;
using ActiveLogging;
using ActiveOptions;
using ActiveOptions.Sqlite;
using ActiveOptions.Azure.Cosmos;
using HQ.Integration.Azure;
using HQ.Platform.Api.Deployment;
using HQ.Platform.Node.Backends;
using HQ.Platform.Node.Clouds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Constants = HQ.Common.Constants;

namespace HQ.Platform.Node
{
	public static class Configure
	{
		private const string AppSettingsFileName = "appsettings";
		private const string AppSettingsFileExtension = ".json";

		public static IWebHostBuilder ConfigureHq(this IWebHostBuilder hostBuilder, string[] args,
			string appName = null, bool seedOnLoad = false)
		{
			hostBuilder.ConfigureAppConfiguration((context, config) =>
			{
				// https://github.com/aspnet/AspNetCore/issues/11921
				if (appName != null)
					context.HostingEnvironment.ApplicationName = appName;

				config.Sources.Clear();
				config.AddJsonFile($"{AppSettingsFileName}{AppSettingsFileExtension}", true, true);
				config.AddJsonFile(
					$"{AppSettingsFileName}.{context.HostingEnvironment.EnvironmentName}{AppSettingsFileExtension}",
					true, true);
				config.AddCloudConfiguration(seedOnLoad);

				if (context.HostingEnvironment.IsDevelopment())
				{
					var assembly = GetApplicationAssembly(context);
					if (assembly != null)
						config.AddUserSecrets(assembly, true);
				}

				config.AddEnvironmentVariables();

				if (args != null && args.Length > 0)
					config.AddCommandLine(args);
			});

			hostBuilder.ConfigureLogging((context, builder) =>
			{
				var config = context.Configuration;

				builder.ClearProviders();
				builder.AddConfiguration(config);

				if (context.HostingEnvironment.IsDevelopment())
					builder.AddConsole(o => { o.IncludeScopes = Debugger.IsAttached; });

				builder.AddDebug();
				builder.AddEventSourceLogger();
				builder.AddTraceLogging();
				builder.AddSafeLogging();
				builder.AddCloudLogging(config);
			});

			return hostBuilder;
		}

		private static void AddCloudLogging(this ILoggingBuilder builder, IConfiguration config)
		{
			var cloudConfig = config.GetSection("Cloud");
			switch (cloudConfig["Provider"])
			{
				case nameof(Azure):
				{
					var cloudOptions = new AzureOptions();
					cloudConfig.FastBind(cloudOptions);
					builder.ConfigureCloudLogging(cloudOptions);
					break;
				}
			}
		}

		private static void AddCloudConfiguration(this IConfigurationBuilder config, bool seedOnLoad)
		{
			config.AddJsonFile("seed.json", true, true);

			var root = config.Build();
			var backend = root.GetSection("Backend");
			var backendType = backend["Type"];
			if (string.IsNullOrWhiteSpace(backendType))
			{
				Trace.TraceWarning("No backend type found!");
				return;
			}

			Trace.TraceInformation($"Installing {backendType} back-end configuration.", backendType);

			var connectionString = backend.GetConnectionString(Constants.Options.DefaultCollection);
			var seed = seedOnLoad ? ConfigurationLoader.FromEmbeddedJsonFile("seed.json") : null;

			static void DefaultSaveOptions(SaveConfigurationOptions o)
			{
				o.MigrateOnStartup = true;
				o.CreateIfNotExists = true;
				o.SeedStrategy = SeedStrategy.InsertIfNotExists;
			}

			switch (backendType)
			{
				case nameof(DocumentDb):
					config.AddCosmosConfigurationProvider(connectionString, true, seed, DefaultSaveOptions);
					break;
				case nameof(Sqlite):
					config.AddSqliteConfigurationProvider(connectionString, true, seed, DefaultSaveOptions);
					break;
				case nameof(SqlServer):
					config.AddSqlServerConfigurationProvider(connectionString, true, seed, DefaultSaveOptions);
					break;
				default:
					throw new ArgumentOutOfRangeException(backendType, typeof(string), null);
			}
		}

		private static Assembly GetApplicationAssembly(WebHostBuilderContext context)
		{
			var assembly = Assembly.Load(new AssemblyName(context.HostingEnvironment.ApplicationName));
			return assembly;
		}
	}
}