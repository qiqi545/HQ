using System;
using System.Data;
using Dapper;
using HQ.Data.SessionManagement;
using HQ.Data.SessionManagement.Sqlite;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Queries;
using HQ.Data.Sql.Sqlite;
using HQ.Data.Sql.Sqlite.Configuration;
using HQ.Extensions.Metrics;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Constants = HQ.Common.Constants;

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
            
            if (scope == ConnectionScope.ByRequest)
            {
                services.AddHttpContextAccessor();
            }

            services.AddDatabaseConnection<SqliteConnectionFactory>(connectionString, scope, Constants.ConnectionSlots.BackgroundTasks);

            services.Replace(ServiceDescriptor.Singleton<IBackgroundTaskStore, SqliteBackgroundTaskStore>());

            var dialect = new SqliteDialect();
            SqlBuilder.Dialect = dialect;

            services.Configure(configureDatabase);

            services.AddMetrics();
            services.TryAddSingleton<ISqlDialect>(dialect);
            services.TryAddSingleton(dialect);

            SqlMapper.AddTypeHandler(DateTimeOffsetHandler.Default);
            SqlMapper.AddTypeHandler(TimeSpanHandler.Default);

            var serviceProvider = services.BuildServiceProvider();

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

        public class TimeSpanHandler : SqlMapper.TypeHandler<TimeSpan?>
        {
            protected TimeSpanHandler() { }

            public static readonly TimeSpanHandler Default = new TimeSpanHandler();

            public override void SetValue(IDbDataParameter parameter, TimeSpan? value)
            {
                if (value.HasValue)
                {
                    parameter.Value = value.Value;
                }
                else
                {
                    parameter.Value = DBNull.Value;
                }
            }

            public override TimeSpan? Parse(object value)
            {
                switch (value)
                {
                    case null:
                        return null;
                    case TimeSpan timeSpan:
                        return timeSpan;
                    default:
                        return TimeSpan.Parse(value.ToString());
                }
            }
        }

        public class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset?>
        {
            protected DateTimeOffsetHandler() { }

            public static readonly DateTimeOffsetHandler Default = new DateTimeOffsetHandler();

            public override void SetValue(IDbDataParameter parameter, DateTimeOffset? value)
            {
                if (value.HasValue)
                {
                    parameter.Value = value.Value;
                }
                else
                {
                    parameter.Value = DBNull.Value;
                }
            }

            public override DateTimeOffset? Parse(object value)
            {
                switch (value)
                {
                    case null:
                        return null;
                    case DateTimeOffset offset:
                        return offset;
                    default:
                        return Convert.ToDateTime(value);
                }
            }
        }
    }
}
