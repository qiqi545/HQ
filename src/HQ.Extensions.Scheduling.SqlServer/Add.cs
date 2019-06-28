using System;
using HQ.Data.SessionManagement;
using HQ.Data.SessionManagement.SqlServer;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Queries;
using HQ.Data.Sql.SqlServer;
using HQ.Data.Sql.SqlServer.Configuration;
using HQ.Extensions.Metrics;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Scheduling.SqlServer
{
    public static class Add
    {
        public static BackgroundTaskBuilder AddSqlServerBackgroundTasksStore(this BackgroundTaskBuilder builder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            IConfiguration databaseConfig = null)
        {
            return builder.AddSqlServerBackgroundTasksStore(connectionString, scope, databaseConfig.Bind);
        }

        public static BackgroundTaskBuilder AddSqlServerBackgroundTasksStore(this BackgroundTaskBuilder builder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            Action<SqlServerOptions> configureDatabase = null)
        {
            var services = builder.Services;

            if(scope == ConnectionScope.ByRequest)
                services.AddHttpContextAccessor();

            services.AddDatabaseConnection<SqlServerConnectionFactory>(connectionString, scope, Common.Constants.ConnectionSlots.BackgroundTasks);

            services.Replace(ServiceDescriptor.Singleton<IBackgroundTaskStore, SqlServerBackgroundTaskStore>());

            var dialect = new SqlServerDialect();
            SqlBuilder.Dialect = dialect;

            services.Configure(configureDatabase);

            services.AddMetrics();
            services.TryAddSingleton<ISqlDialect>(dialect);
            services.TryAddSingleton(dialect);

            var serviceProvider = services.BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<BackgroundTaskOptions>>();

            MigrateToLatest(connectionString, options.Value);

            return builder;
        }

        private static void MigrateToLatest(string connectionString, BackgroundTaskOptions options)
        {
            var runner = new SqlServerMigrationRunner(connectionString);

            if (options.Store.CreateIfNotExists)
            {
                runner.CreateDatabaseIfNotExists();
            }

            if (options.Store.MigrateOnStartup)
            {
                runner.MigrateUp(typeof(CreateBackgroundTasksSchema).Assembly, typeof(CreateBackgroundTasksSchema).Namespace);
            }
        }
    }
}
