using HQ.Data.SessionManagement;
using HQ.Integration.SqlServer.Scheduling;
using HQ.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Extensions.Scheduling.Tests.SqlServer
{
    public class SchedulingSqlServerFixture : SqlServerFixture
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddBackgroundTasks(o => { })
                .AddSqlServerBackgroundTasksStore(ConnectionString, ConnectionScope.ByThread, o => { });
        }
    }
}
