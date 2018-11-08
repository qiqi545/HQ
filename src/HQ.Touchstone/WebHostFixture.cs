using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using HQ.Touchstone.Harmony;
using HQ.Touchstone.Harmony.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Touchstone
{
	public sealed class WebHostFixture<T> : IDisposable where T : class
	{
		internal const string EnvironmentNameVariable = "ASPNETCORE_ENVIRONMENT";

		internal const string DefaultAppSettingsFileName = "appsettings";
		internal const string DefaultAppSettingsFileExtension = ".json";
		internal const string DefaultAppSettingsFilePath = DefaultAppSettingsFileName + DefaultAppSettingsFileExtension;

		internal readonly TestServer Server;

		public WebHostFixture(SystemUnderTest<T> systemUnderTest)
		{
			var config = new[] { new Action<IConfiguration>(systemUnderTest.Configuration) };
			var configureServices = new[] { new Action<IServiceCollection>(systemUnderTest.ConfigureServices) };
			var configure = new[] { new Action<IApplicationBuilder>(systemUnderTest.Configure) };

			var b = Create(config, configureServices, configure);

			Server = new TestServer(b);
		}

		public HttpClient CreateClient()
		{
			return Server.CreateClient();
		}

		private static void ConfigureAppConfiguration(IHostingEnvironment env, TestSettings testSettings, IConfigurationBuilder c)
		{
			if (!string.IsNullOrWhiteSpace(testSettings.AppSettingsPath))
			{
				var appSettingsPath = testSettings.AppSettingsPath;				
				var attributes = File.GetAttributes(appSettingsPath);

				string appSettingsFileName;
				string appSettingsFileExtension;

				if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
				{
					appSettingsFileName = DefaultAppSettingsFileName;
					appSettingsFileExtension = DefaultAppSettingsFileExtension;
				}
				else
				{
					appSettingsFileName = Path.GetFileNameWithoutExtension(appSettingsPath);
					appSettingsFileExtension = Path.GetExtension(appSettingsPath);
					appSettingsPath = Path.GetDirectoryName(appSettingsPath);
				}

				Debug.Assert(appSettingsPath != null);
				var appSettingsDefaultPath = Path.GetFullPath(Path.Combine(appSettingsPath, string.Concat(appSettingsFileName, appSettingsFileExtension)));
				var appSettingsEnvPath = Path.GetFullPath(Path.Combine(appSettingsPath, string.Concat(appSettingsFileName, ".", testSettings.EnvironmentName, appSettingsFileExtension)));

				c.AddJsonFile(appSettingsDefaultPath, optional: false, reloadOnChange: true);
				c.AddJsonFile(appSettingsEnvPath, optional: true);
			}
			else
			{
				c.AddJsonFile(DefaultAppSettingsFilePath, optional: true, reloadOnChange: true);
				c.AddJsonFile($"{DefaultAppSettingsFileName}.{env.EnvironmentName}{DefaultAppSettingsFileExtension}", optional: true);
			}

			c.AddEnvironmentVariables();
		}

		private static IWebHostBuilder Create
		(
			ICollection<Action<IConfiguration>> configuration,
			ICollection<Action<IServiceCollection>> configureServices,
			ICollection<Action<IApplicationBuilder>> configure
		)
		{
			T startup = null;
			IConfiguration config = null;
			IHostingEnvironment env;
			IServiceCollection services = null;

			var builder = new WebHostBuilder()
				.ConfigureAppConfiguration((context, cb) =>
				{
					env = context.HostingEnvironment;
					var testSettings = BuildTestSettings(env.ContentRootPath, env.EnvironmentName);
					ConfigureAppConfiguration(env, testSettings, cb);
					config = cb.Build();
				})
				.ConfigureServices(serviceCollection =>
				{
					services = serviceCollection;

					Debug.Assert(services != null);
					Debug.Assert(config != null);

					var serviceProvider = serviceCollection.BuildServiceProvider();
					Debug.Assert(serviceProvider != null);

					using (var container = new HarmonyContainer(serviceProvider))
					{
						container.AddAspNetCore();

						container.Register(serviceProvider);
						container.Register(services);
						container.Register(config);

						startup = container.Resolve<T>();
						Debug.Assert(startup != null);
						foreach (var action in configuration)
							action(config);
						
						container.Register(startup);
						container.InvokeMethod<T>("ConfigureServices");
						foreach (var action in configureServices)
							action(serviceCollection);
					}

				})
				.Configure(app =>
				{
					Debug.Assert(services != null);
					Debug.Assert(config != null);
					Debug.Assert(startup != null);
					Debug.Assert(app != null);

					var serviceProvider = services.BuildServiceProvider();
					Debug.Assert(serviceProvider != null);

					using (var container = new HarmonyContainer(serviceProvider))
					{
						container.AddAspNetCore();

						container.Register(serviceProvider);
						container.Register(services);
						container.Register(config);
						container.Register(startup);
						container.Register(app);

						container.InvokeMethod<T>("Configure");
						foreach (var action in configure)
							action(app);
					}
				});

			var applicationKey = builder.GetSetting(WebHostDefaults.ApplicationKey);
			var assemblyName = applicationKey ?? typeof(T).Assembly.GetName().Name;
			if (applicationKey != assemblyName)
				builder.UseSetting(WebHostDefaults.ApplicationKey, assemblyName);

			return builder;
		}

		private static TestSettings BuildTestSettings(string contentRootPath, string environmentName)
		{
			var testConfig = new ConfigurationBuilder()
				.SetBasePath(contentRootPath)
				.AddJsonFile($"testsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"testsettings.{environmentName}.json", optional: true)
				.Build();

			var testSettings = new TestSettings();
			testConfig.Bind(testSettings);
			testSettings.EnvironmentName = Environment.GetEnvironmentVariable(EnvironmentNameVariable) ??
			                               testSettings.EnvironmentName;
			return testSettings;
		}
		
		public void Dispose()
		{
			Server?.Dispose();
		}
	}
}
