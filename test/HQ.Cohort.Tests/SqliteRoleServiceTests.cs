using System;
using System.IO;
using HQ.Cohort.Stores.Sql.Sqlite;
using HQ.Connect;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Cohort.Tests
{
    public class SqliteRoleServiceTests : RoleServiceTests
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.Stores.CreateIfNotExists = true;
                    options.Stores.MigrateOnStartup = true;
                })
                .AddSqliteIdentityStore<IdentityUser, IdentityRole>($"Data Source={Guid.NewGuid()}.db",
                    ConnectionScope.KeepAlive);
        }

        protected override void Dispose(bool disposing)
        {
            var connection = serviceProvider.GetRequiredService<IDataConnection>();
            if(connection.Current is SqliteConnection sqlite)
                File.Delete(sqlite.DataSource);
        }
    }
}
