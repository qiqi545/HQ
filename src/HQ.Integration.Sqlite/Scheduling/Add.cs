using System;
using Dapper;
using HQ.Common;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Queries;
using HQ.Extensions.DependencyInjection.AspNetCore;
using HQ.Extensions.Metrics;
using HQ.Extensions.Options;
using HQ.Extensions.Scheduling;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Models;
using HQ.Integration.Sqlite.SessionManagement;
using HQ.Integration.Sqlite.Sql;
using HQ.Integration.Sqlite.Sql.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Constants = HQ.Common.Constants;

namespace HQ.Integration.Sqlite.Scheduling
{
    public static class Add
    {
        public static BackgroundTaskBuilder AddSqliteBackgroundTasksStore(this BackgroundTaskBuilder builder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            IConfiguration databaseConfig = null)
        {
            return builder.AddSqliteBackgroundTasksStore(connectionString, scope, databaseConfig.FastBind);
        }

        public static BackgroundTaskBuilder AddSqliteBackgroundTasksStore(this BackgroundTaskBuilder builder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            Action<SqliteOptions> configureDatabase = null)
        {
            if (scope == ConnectionScope.ByRequest)
				builder.Services.AddHttpContextAccessor();

            var extensions = new[] {new HttpAccessorExtension()};

			builder.Services.AddDatabaseConnection<BackgroundTaskBuilder, SqliteConnectionFactory>(connectionString, scope, extensions);
			builder.Services.AddLocalTimestamps();
			builder.Services.Replace(ServiceDescriptor.Singleton<IBackgroundTaskStore, SqliteBackgroundTaskStore>());

            var dialect = new SqliteDialect();
            SqlBuilder.Dialect = dialect;

            builder.Services.Configure(configureDatabase);
            builder.Services.AddMetrics();
            builder.Services.TryAddSingleton<ISqlDialect>(dialect);
            builder.Services.TryAddSingleton(dialect);

            SqlMapper.AddTypeHandler(DateTimeOffsetHandler.Default);
            SqlMapper.AddTypeHandler(TimeSpanHandler.Default);

            var serviceProvider = builder.Services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<BackgroundTaskOptions>>();

            MigrateToLatest(connectionString, options.Value);

            return builder;
        }

        private static void MigrateToLatest(string connectionString, BackgroundTaskOptions options)
        {
            var runner = new SqliteMigrationRunner(connectionString);

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
