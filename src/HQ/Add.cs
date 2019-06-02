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
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Swashbuckle.AspNetCore.Swagger;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.SessionManagement;
using HQ.Platform.Api;
using HQ.Platform.Identity;
using HQ.Platform.Identity.AspNetCore.Mvc;
using HQ.Platform.Identity.Models;
using HQ.Platform.Identity.Stores.Sql.DocumentDb;
using HQ.Platform.Identity.Stores.Sql.Sqlite;
using HQ.Platform.Identity.Stores.Sql.SqlServer;
using HQ.Platform.Operations;
using HQ.Platform.Security.AspNetCore;
using Lime;
using Lime.Web;
using Lime.Web.SemanticUi;
using HQ.Template.UI.Pages;
using HQ.UI.Pages;

namespace HQ.Template
{
    public static class Add
    {
        public static IServiceCollection AddHq(this IServiceCollection services, IHostingEnvironment env, IConfigurationRoot configurationRoot, Action<MvcOptions> setupAction = null)
        {
            var config = configurationRoot.GetSection("HQ");

            services.AddSingleton(configurationRoot);
            services.TryAddSingleton<IServerTimestampService, LocalServerTimestampService>();

            services.AddSecurityPolicies(config.GetSection("Security"));
            services.AddOperationsApi(env, config.GetSection("Ops"));
            services.AddPlatformApi(config.GetSection("Api"));
            services.AddMultiTenancy<IdentityTenant, IdentityApplication>(config.GetSection("Api").GetSection("MultiTenancy"))
                .AddIdentityTenantContextStore<IdentityTenant>()
                .AddIdentityApplicationContextStore<IdentityApplication>();
            services.AddVersioning(config.GetSection("Api").GetSection("Versioning"));
            services.AddDynamicMvc(setupAction)
                .AddIdentityApi<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication, string>(
                    config.GetSection("Identity").GetSection("Api"), config.GetSection("Security"));

            return services;
        }

        public static IServiceCollection AddBackendServices(this IServiceCollection services, string databaseType, string connectionString, IConfiguration dbConfig, IConfiguration config)
        {
            var identity = services
                .AddIdentityExtended<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication, string>(
                    config.GetSection("Identity"));

            switch (databaseType)
            {
                case "DocumentDb":
                    identity.AddDocumentDbIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication>(
                        connectionString, ConnectionScope.ByRequest,
                        dbConfig);
                    break;
                case "SqlServer":
                    identity.AddSqlServerIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication>(
                        connectionString, ConnectionScope.ByRequest,
                        dbConfig);
                    break;
                case "Sqlite":
                    identity.AddSqliteIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication>(
                        connectionString, ConnectionScope.ByRequest,
                        dbConfig);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }

            UiConfig.Settings = settings =>
            {
                settings.DefaultPageTitle = Assembly.GetCallingAssembly().GetName().Name;
                settings.ComponentAssemblies = new[]
                {
                    typeof(UiComponent).Assembly,   // Lime
                    typeof(HtmlSystem).Assembly,    // Lime.Web
                    typeof(SemanticUi).Assembly,    // Lime.Web.Semantic.UI,
                    typeof(Dashboard).Assembly,     // HQ.UI
                    Assembly.GetEntryAssembly()     // App
                };
            };

            services.AddLimeUi(services.BuildServiceProvider().GetRequiredService<IHostingEnvironment>(),
                typeof(SemanticUi).Assembly);

            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.SwaggerDoc("swagger", new Info
                {
                    Title = "Sample API",
                    Version = "v1"
                });
                c.DescribeAllEnumsAsStrings();
            });

            return services;
        }
    }
}
