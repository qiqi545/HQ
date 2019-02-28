using System;
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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ
{
    public static class Installer
    {
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
                case DatabaseType.CosmosDb:
                    identity.AddDocumentDbIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant>(
                        config.GetConnectionString("DefaultConnection"),
                        config.GetSection("DbOptions"));
                    break;
                case DatabaseType.SqlServer:
                    identity.AddSqlServerIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant>(
                        config.GetConnectionString("DefaultConnection"),
                        config.GetSection("DbOptions"));
                    break;
                case DatabaseType.MySql:
                    identity.AddMySqlIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant>(
                        config.GetConnectionString("DefaultConnection"),
                        config.GetSection("DbOptions"));
                    break;
                case DatabaseType.Sqlite:
                    identity.AddSqliteIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant>(
                        config.GetConnectionString("DefaultConnection"),
                        config.GetSection("DbOptions"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }

            services.AddSecurityPolicies(config.GetSection("Security"));

            return services;
        }

        public static IApplicationBuilder UseHq(this IApplicationBuilder app, Action<IRouteBuilder> configureRoutes = null)
        {
            app.UsePublicApi();

            app.UseDevOpsApi(configureRoutes);

            app.UseMultiTenancy<IdentityTenant, string>();

            app.UseIdentityApi();

            return app;
        }
    }

    public enum DatabaseType
    {
        CosmosDb,
        SqlServer,
        MySql,
        Sqlite
    }
}
