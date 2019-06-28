using System;
using System.Data.DocumentDb;
using HQ.Common;
using HQ.Data.SessionManagement;
using HQ.Data.SessionManagement.DocumentDb;
using HQ.Data.Sql.DocumentDb;
using HQ.Extensions.Metrics;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Scheduling.DocumentDb
{
    public static class Add
    {
        public static BackgroundTaskBuilder AddDocumentDbBackgroundTasksStore(this BackgroundTaskBuilder builder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            IConfiguration databaseConfig = null)
        {
            return builder.AddDocumentDbBackgroundTasksStore(connectionString, scope, databaseConfig.Bind);
        }

        public static BackgroundTaskBuilder AddDocumentDbBackgroundTasksStore(this BackgroundTaskBuilder builder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            Action<DocumentDbOptions> configureDatabase = null)
        {
            Bootstrap.EnsureInitialized();

            var services = builder.Services;
            
            if (scope == ConnectionScope.ByRequest)
            {
                services.AddHttpContextAccessor();
            }

            services.Replace(ServiceDescriptor.Singleton<IBackgroundTaskStore, DocumentBackgroundTaskStore>());
            services.Configure(configureDatabase);

            services.AddMetrics();
            var serviceProvider = services.BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<BackgroundTaskOptions>>();

            var dbOptions = new DocumentDbOptions();
            configureDatabase?.Invoke(dbOptions);

            MigrateToLatest(connectionString, options.Value, dbOptions);

            return builder;
        }

        private static void MigrateToLatest(string connectionString, BackgroundTaskOptions taskOptions, DocumentDbOptions dbOptions)
        {
            var runner = new DocumentDbMigrationRunner(connectionString);

            if (taskOptions.Store.CreateIfNotExists)
            {
                runner.CreateDatabaseIfNotExistsAsync().GetAwaiter().GetResult();
            }

            if (taskOptions.Store.MigrateOnStartup)
            {
                var builder = new DocumentDbConnectionStringBuilder(connectionString);
                var client = builder.Build();

                var schema = new CreateBackgroundTasksSchema(client, dbOptions);
                schema.Up().GetAwaiter().GetResult();
            }
        }
    }
}
