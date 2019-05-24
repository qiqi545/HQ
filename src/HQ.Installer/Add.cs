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
using Blowdart.UI;
using Blowdart.UI.Web;
using Blowdart.UI.Web.SemanticUI;
using Demo.UI;
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Installer
{
    public static class Add
    {
        public static IServiceCollection AddHq(this IServiceCollection services, DatabaseType databaseType, IConfiguration config, Action<MvcOptions> setupAction = null)
        {
            services.TryAddSingleton<IServerTimestampService, LocalServerTimestampService>();

            services.AddSecurityPolicies(config.GetSection("Security"));
            services.AddOperationsApi(config.GetSection("Ops"));
            services.AddPlatformApi(config.GetSection("Api"));
            services.AddMultiTenancy<IdentityTenant>(config.GetSection("Api").GetSection("MultiTenancy")).AddIdentityTenantContextStore<IdentityTenant>();
            services.AddVersioning(config.GetSection("Api").GetSection("Versioning")).AddIdentityVersionContextStore();

            var identity = services
                .AddIdentityExtended<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, string>(
                    config.GetSection("Identity"));

            services.AddDynamicMvc(setupAction)
               .AddIdentityApi<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, string>(
                   config.GetSection("Identity").GetSection("Api"), config.GetSection("Security"));

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
                case DatabaseType.Sqlite:
                    identity.AddSqliteIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant>(
                        config.GetConnectionString("DefaultConnection"), ConnectionScope.ByRequest,
                        config.GetSection("DbOptions"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }

            return services;
        }

        public static void AddUi(this IServiceCollection services)
        {
            UiConfig.Settings = settings =>
            {
                settings.DefaultPageTitle = Assembly.GetCallingAssembly().GetName().Name;
                settings.ComponentAssemblies = new[]
                {
                    typeof(UiComponent).Assembly,   // Blowdart.UI
                    typeof(HtmlSystem).Assembly,    // Blowdart.UI.Web
                    typeof(SemanticUi).Assembly,    // Blowdart.UI.Web.Semantic.UI,
                    typeof(Dashboard).Assembly,     // HQ
                    Assembly.GetEntryAssembly()     // App
                };
            };

            services.AddBlowdartUi(services.BuildServiceProvider().GetRequiredService<IHostingEnvironment>(),
                typeof(SemanticUi).Assembly);
        }
    }
}
