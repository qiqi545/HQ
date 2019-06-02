using System;
using System.IO;
using HQ.Platform.Identity.Models;
using HQ.Platform.Identity.Stores.Sql.Sqlite;
using HQ.Data.SessionManagement;
using HQ.Test.Sdk;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Identity.Tests.Sqlite
{
    public class SqliteFixture : IServiceFixture
    {
        public IServiceProvider ServiceProvider { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityExtended<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication, string>(options =>
                {
                    options.User.RequirePhoneNumber = false;
                    options.User.RequireEmail = false;

                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireDigit = false;

                    options.Stores.CreateIfNotExists = true;
                    options.Stores.MigrateOnStartup = true;
                })
                .AddSqliteIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication>($"Data Source={Guid.NewGuid()}.db",
                    ConnectionScope.KeepAlive);
        }

        public void Dispose()
        {
            var connection = ServiceProvider?.GetRequiredService<IDataConnection>();
            if (connection?.Current is WrapDbConnection wrapped && wrapped.Inner is SqliteConnection sqlite)
            {
                sqlite.Close();
                sqlite.Dispose();

                GC.Collect();
                GC.WaitForPendingFinalizers();

                File.Delete(sqlite.DataSource);
            }
        }
    }
}
