using System;
using System.IO;
using HQ.Data.SessionManagement;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Test.Sdk.Data
{
    public abstract class SqliteFixture : IServiceFixture
    {
        public IServiceProvider ServiceProvider { get; set; }

        public virtual void ConfigureServices(IServiceCollection services) { }

        public void Dispose()
        {
            var connection = ServiceProvider?.GetRequiredService<IDataConnection>();
            if (!(connection?.Current is WrapDbConnection wrapped) || !(wrapped.Inner is SqliteConnection sqlite))
                return;

            sqlite.Close();
            sqlite.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            if(sqlite.DataSource != null)
                File.Delete(sqlite.DataSource);
        }
    }
}
