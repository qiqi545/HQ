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
using Blowdart.UI;
using Blowdart.UI.Web;
using Blowdart.UI.Web.SemanticUI;
using HQ.Common;
using HQ.Data.SessionManagement;
using HQ.Extensions.Logging;
using HQ.Installer.UI;
using HQ.Platform.Api;
using HQ.Platform.Identity;
using HQ.Platform.Identity.AspNetCore.Mvc;
using HQ.Platform.Identity.Models;
using HQ.Platform.Identity.Stores.Sql.DocumentDb;
using HQ.Platform.Identity.Stores.Sql.MySql;
using HQ.Platform.Identity.Stores.Sql.Sqlite;
using HQ.Platform.Identity.Stores.Sql.SqlServer;
using HQ.Platform.Operations;
using HQ.Platform.Security.AspNetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Installer
{
    public static class HqServer
    {
        public static void Start<TStartup>(string[] args) where TStartup : class
        {
            try
            {
                Trace.Listeners.Add(new ActionTraceListener(Console.Write, Console.WriteLine));
                Trace.TraceInformation(args == null || args.Length == 0
                    ?  "HQ started."
                    : $"HQ started with args: {string.Join(" ", args)}");

                var builder = UseStartup<TStartup>(args);
                var host = builder.Build();
                host.Run();

                Trace.TraceInformation("HQ stopped normally.");
            }
            catch (Exception exception)
            {
                Trace.TraceError("HQ stopped unexpectedly. Error: {0}", exception);

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                else if (Environment.UserInteractive)
                {
                    Console.WriteLine("Press any key to quit.");
                    Console.ReadKey();
                }
            }
        }

        private static IWebHostBuilder UseStartup<TStartup>(string[] args) where TStartup : class
        {
            var builder = WebHost.CreateDefaultBuilder(args);

            return builder.UseStartup<TStartup>();
        }

        public static IServiceCollection AddHq(this IServiceCollection services, DatabaseType databaseType, IConfiguration config,
            Action<MvcOptions> setupAction = null)
        {
            services.AddPublicApi(config.GetSection("Api"));

            services.AddDevOpsApi(config.GetSection("Ops"));

            services.AddMultiTenancy<IdentityTenant>(config.GetSection("Tenants"))
                .AddIdentityTenantContextStore<IdentityTenant>();

            var identity = services
                .AddIdentityExtended<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, string>(
                    config.GetSection("Identity"));

            services.AddDynamicMvc(setupAction)
               .AddIdentityApi<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, string>(
                   config.GetSection("IdentityApi"), config.GetSection("Security"));

            switch (databaseType)
            {
                case DatabaseType.DocumentDb:
                    identity.AddDocumentDbIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant>(
                        config.GetConnectionString("DefaultConnection"), ConnectionScope.ByRequest,
                        config.GetSection("DbOptions"));
                    break;
                case DatabaseType.SqlServer:
                    identity.AddSqlServerIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant>(
                        config.GetConnectionString("DefaultConnection"), ConnectionScope.ByRequest,
                        config.GetSection("DbOptions"));
                    break;
                case DatabaseType.MySql:
                    identity.AddMySqlIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant>(
                        config.GetConnectionString("DefaultConnection"), ConnectionScope.ByRequest,
                        config.GetSection("DbOptions"));
                    break;
                case DatabaseType.Sqlite:
                    identity.AddSqliteIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant>(
                        config.GetConnectionString("DefaultConnection"), ConnectionScope.ByRequest,
                        config.GetSection("DbOptions"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }

            services.AddSecurityPolicies(config.GetSection("Security"));

            return services;
        }

        public static void AddUi(this IServiceCollection services)
        {
            var title = Assembly.GetCallingAssembly().GetName().Name;

            UiConfig.Settings = settings =>
            {
                settings.Title = title;
                settings.System = new SemanticUi();
                settings.ComponentAssemblies = new[]
                {
                    typeof(UiComponent).Assembly,       // Blowdart.UI
                    typeof(HtmlSystem).Assembly,        // Blowdart.UI.Web
                    typeof(SemanticUi).Assembly,        // Blowdart.UI.Web.Semantic.UI,
                    typeof(SplashPage).Assembly,        // HQ
                    Assembly.GetEntryAssembly()         // App
                };
            };

            var serviceProvider = services.BuildServiceProvider();
            var env = serviceProvider.GetRequiredService<IHostingEnvironment>();

            services.AddBlowdartUi(env, typeof(SemanticUi).Assembly);
        }

        public static IApplicationBuilder UseHq(this IApplicationBuilder app, Action<IRouteBuilder> configureRoutes = null)
        {
            Bootstrap.EnsureInitialized();

            app.UsePublicApi();
            app.UseDevOpsApi(configureRoutes);
            app.UseMultiTenancy<IdentityTenant, string>();
            app.UseAuthentication();

            Program.MastHead();

            if (app.ApplicationServices.GetService(typeof(LayoutRoot)) == null)
                return app;

            app.UseBlowdartUi(site =>
            {
                site.Default(ui =>
                {
                    ui.Component("SplashPage");
                });
            });

            return app;
        }
    }
}
