using System;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Queries;
using HQ.Extensions.Logging;
using HQ.Extensions.Metrics;
using HQ.Extensions.Scheduling;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Models;
using HQ.Integration.SqlServer.SessionManagement;
using HQ.Integration.SqlServer.Sql;
using HQ.Integration.SqlServer.Sql.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace HQ.Integration.SqlServer.Scheduling
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

            services.AddLocalTimestamps();
			services.AddSafeLogging();
            services.AddDatabaseConnection<BackgroundTaskBuilder, SqlServerConnectionFactory>(connectionString, scope);
            
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
