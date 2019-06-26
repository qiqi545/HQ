using System;
using HQ.Data.SessionManagement;
using HQ.Extensions.Scheduling.Sqlite;
using HQ.Test.Sdk.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Extensions.Scheduling.Tests.Sqlite
{
    public class SchedulingSqliteFixture : SqliteFixture
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddBackgroundTasks(o => { })
                .AddSqliteBackgroundTasksStore($"Data Source={Guid.NewGuid()}.db", ConnectionScope.KeepAlive, o => { });
        }
    }
}
