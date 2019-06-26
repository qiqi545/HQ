using System;
using HQ.Platform.Identity.Models;
using HQ.Platform.Identity.Stores.Sql.Sqlite;
using HQ.Data.SessionManagement;
using HQ.Platform.Security.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HQ.Test.Sdk.Data;

namespace HQ.Platform.Identity.Tests.Sqlite
{
    public class IdentitySqliteFixture : SqliteFixture
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<SecurityOptions>(o => { });
            services.AddHttpContextAccessor();
            services.AddIdentityExtended<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication, string>(o =>
                {
                    o.User.RequirePhoneNumber = false;
                    o.User.RequireEmail = false;

                    o.Password.RequireNonAlphanumeric = false;
                    o.Password.RequireDigit = false;

                    o.Stores.CreateIfNotExists = true;
                    o.Stores.MigrateOnStartup = true;
                })
                .AddSqliteIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication>($"Data Source={Guid.NewGuid()}.db",
                    ConnectionScope.KeepAlive);
        }
    }
}