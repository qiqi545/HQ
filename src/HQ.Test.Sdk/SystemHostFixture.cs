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
using System.IO;
using System.Net.Http;
using HQ.Extensions.DependencyInjection;
using HQ.Extensions.DependencyInjection.AspNetCore;
using HQ.Test.Sdk.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace HQ.Test.Sdk
{
    public sealed class SystemHostFixture<T> : IFunctionsHostBuilder, IDisposable where T : class
    {
        internal const string DefaultAppSettingsFileName = "appsettings";
        internal const string DefaultAppSettingsFileExtension = ".json";
        internal const string DefaultAppSettingsFilePath = DefaultAppSettingsFileName + DefaultAppSettingsFileExtension;

        internal readonly TestServer Server;

        public IServiceCollection Services { get; private set; }

        public SystemHostFixture(SystemUnderTest<T> systemUnderTest, SystemTopology topology)
        {
            var config = new[] {new Action<IConfiguration>(systemUnderTest.Configuration)};
            var configureServices = new[] {new Action<IServiceCollection>(systemUnderTest.ConfigureServices)};
            var configure = new[] {new Action<IApplicationBuilder>(systemUnderTest.Configure)};

            Server = new TestServer(CreateWebHostBuilder(config, configureServices, configure, topology));
        }

        public void Dispose()
        {
            Server?.Dispose();
        }

        public HttpClient CreateClient()
        {
            return Server.CreateClient();
        }

        private static void ConfigureAppConfiguration(IHostingEnvironment env, TestSettings testSettings,
            IConfigurationBuilder c)
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

                if (appSettingsPath != null)
                {
                    var appSettingsDefaultPath = Path.GetFullPath(Path.Combine(appSettingsPath,
                        string.Concat(appSettingsFileName, appSettingsFileExtension)));
                    var appSettingsEnvPath = Path.GetFullPath(Path.Combine(appSettingsPath,
                        string.Concat(appSettingsFileName, ".", testSettings.EnvironmentName,
                            appSettingsFileExtension)));

                    c.AddJsonFile(appSettingsDefaultPath, false, true);
                    c.AddJsonFile(appSettingsEnvPath, true);
                }
            }
            else
            {
                c.AddJsonFile(DefaultAppSettingsFilePath, true, true);
                c.AddJsonFile($"{DefaultAppSettingsFileName}.{env.EnvironmentName}{DefaultAppSettingsFileExtension}",
                    true);
            }

            c.AddEnvironmentVariables();
        }

        private IWebHostBuilder CreateWebHostBuilder
        (ICollection<Action<IConfiguration>> configuration,
            ICollection<Action<IServiceCollection>> configureServices,
            ICollection<Action<IApplicationBuilder>> configure,
            SystemTopology topology)
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

                    using (var container = new DependencyContainer(serviceProvider))
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

                        var methodName = topology == SystemTopology.Web ? "ConfigureServices" : nameof(FunctionsStartup.Configure);
                        container.InvokeMethod<T>(methodName);
                        foreach (var action in configureServices)
                            action(serviceCollection);
                    }
                });

            if (topology == SystemTopology.Web)
            {
                builder.Configure(app =>
                {
                    Debug.Assert(services != null);
                    Debug.Assert(config != null);
                    Debug.Assert(startup != null);
                    Debug.Assert(app != null);

                    var serviceProvider = services.BuildServiceProvider();
                    Debug.Assert(serviceProvider != null);

                    using (var container = new DependencyContainer(serviceProvider))
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
            }

            UseAssemblyForSetting<T>(builder, WebHostDefaults.ApplicationKey);
            UseAssemblyForSetting<T>(builder, WebHostDefaults.StartupAssemblyKey);

            Services = services;

            return builder;
        }

        private static void UseAssemblyForSetting<TStartup>(IWebHostBuilder builder, string key)
        {
            var applicationKey = builder.GetSetting(key);
            var assemblyName = applicationKey ?? typeof(TStartup).Assembly.GetName().Name;
            if (applicationKey != assemblyName)
                builder.UseSetting(key, assemblyName);
        }

        private static TestSettings BuildTestSettings(string contentRootPath, string environmentName)
        {
            var testConfig = new ConfigurationBuilder()
                .SetBasePath(contentRootPath)
                .AddJsonFile("testsettings.json", true, true)
                .AddJsonFile($"testsettings.{environmentName}.json", true)
                .Build();

            var testSettings = new TestSettings();
            testConfig.Bind(testSettings);
            testSettings.EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                                           testSettings.EnvironmentName;
            return testSettings;
        }
    }
}
