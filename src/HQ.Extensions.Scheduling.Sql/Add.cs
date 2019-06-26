using HQ.Common;
using HQ.Data.SessionManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Extensions.Scheduling.Sql
{
    public static class Add
    {
        public static BackgroundTaskBuilder AddSqlStores<TDatabase>(this BackgroundTaskBuilder builder, string connectionString, ConnectionScope scope)
            where TDatabase : class, IConnectionFactory, new()
        {
            var services = builder.Services;

            if (scope == ConnectionScope.ByRequest)
            {
                services.AddHttpContextAccessor();
            }

            services.AddDatabaseConnection<TDatabase>(connectionString, scope, Constants.ConnectionSlots.BackgroundTasks);

            services.TryAddSingleton<SqlBackgroundTaskStore>();
            return builder;
        }
    }
}
