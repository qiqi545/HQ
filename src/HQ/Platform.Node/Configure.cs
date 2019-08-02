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
using System.Linq;
using System.Reflection;
using HQ.Extensions.Deployment;
using HQ.Extensions.Logging;
using HQ.Extensions.Options;
using HQ.Integration.Sqlite.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HQ.Platform.Node
{
    public static class Configure
    {
        private const string AppSettingsFileName = "appsettings";
        private const string AppSettingsFileExtension = ".json";

        public static IWebHostBuilder ConfigureHq(this IWebHostBuilder hostBuilder, string[] args,
            bool seedOnLoad = false, Func<IConfiguration, ICloudOptions[]> clouds = null)
        {
            hostBuilder.ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear();
				config.AddSqlite("settings.db", false, true, seedOnLoad ? ConfigurationLoader.FromEmbeddedJsonFile("seed.json") : null);
                config.AddJsonFile($"{AppSettingsFileName}{AppSettingsFileExtension}", true, true);
                config.AddJsonFile($"{AppSettingsFileName}.{context.HostingEnvironment.EnvironmentName}{AppSettingsFileExtension}", true, true);

                if (context.HostingEnvironment.IsDevelopment())
                {
                    var assembly = GetApplicationAssembly(context);
                    if (assembly != null)
						config.AddUserSecrets(assembly, true);
				}

                config.AddEnvironmentVariables();

                if (args != null && args.Length > 0)
                {
                    config.AddCommandLine(args);
                }
            });

            hostBuilder.ConfigureLogging((context, builder) =>
            {
                var config = context.Configuration;

                builder.ClearProviders();
                builder.AddConfiguration(config);

                if (context.HostingEnvironment.IsDevelopment())
                {
                    builder.AddConsole(o => { o.IncludeScopes = Debugger.IsAttached; });
                }

                builder.AddDebug();
                builder.AddEventSourceLogger();
                builder.AddSafeLogging();

                foreach (var cloud in clouds?.Invoke(config) ?? Enumerable.Empty<ICloudOptions>())
                {
                    builder.ConfigureCloudLogging(cloud);
                }
            });

            return hostBuilder;
        }

        private static Assembly GetApplicationAssembly(WebHostBuilderContext context)
        {
            var assembly = Assembly.Load(new AssemblyName(context.HostingEnvironment.ApplicationName));
            return assembly;
        }
    }
}
