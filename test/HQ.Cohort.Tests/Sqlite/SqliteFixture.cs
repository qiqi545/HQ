using System;
using System.IO;
using HQ.Cohort.Models;
using HQ.Cohort.Stores.Sql.Sqlite;
using HQ.Connect;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Cohort.Tests.Sqlite
{
    public class SqliteFixture : IDisposable
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentity<IdentityUserExtended, IdentityRoleExtended>(options =>
                {
                    options.Stores.CreateIfNotExists = true;
                    options.Stores.MigrateOnStartup = true;
                })
                .AddSqliteIdentityStore<IdentityUserExtended, IdentityRoleExtended>($"Data Source={Guid.NewGuid()}.db",
                    ConnectionScope.KeepAlive);
        }

        public void Dispose()
        {
            var connection = ServiceProvider?.GetRequiredService<IDataConnection>();
            if (connection?.Current is WrapDbConnection wrapped && wrapped.Inner is SqliteConnection sqlite)
            {
                sqlite.Close();
                sqlite.Dispose();
                File.Delete(sqlite.DataSource);
            }
        }

        public IServiceProvider ServiceProvider { get; set; }
    }
}
