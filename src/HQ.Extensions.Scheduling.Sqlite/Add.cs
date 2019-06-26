using System;
using HQ.Data.SessionManagement;
using HQ.Data.SessionManagement.Sqlite;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Queries;
using HQ.Data.Sql.Sqlite;
using HQ.Data.Sql.Sqlite.Configuration;
using HQ.Extensions.Metrics;
using HQ.Extensions.Scheduling.Sql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Extensions.Scheduling.Sqlite
{
    public static class Add
    {
        public static BackgroundTaskBuilder AddSqliteBackgroundTasksStore(this BackgroundTaskBuilder builder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            IConfiguration databaseConfig = null)
        {
            return builder.AddSqliteBackgroundTasksStore(connectionString, scope, databaseConfig.Bind);
        }

        public static BackgroundTaskBuilder AddSqliteBackgroundTasksStore(this BackgroundTaskBuilder builder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            Action<SqliteOptions> configureDatabase = null)
        {
            var services = builder.Services;

            builder.AddSqlStores<SqliteConnectionFactory>(connectionString, scope);

            var dialect = new SqliteDialect();
            SqlBuilder.Dialect = dialect;

            services.Configure(configureDatabase);

            services.AddMetrics();
            services.TryAddSingleton<ISqlDialect>(dialect);
            services.TryAddSingleton(dialect);

            return builder;
        }
    }
}
