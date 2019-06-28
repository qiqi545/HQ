using System;
using System.Data.DocumentDb;
using HQ.Common;
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
        public static BackgroundTaskBuilder AddDocumentDbBackgroundTasksStore(this BackgroundTaskBuilder builder, IConfiguration databaseConfig = null)
        {
            return builder.AddDocumentDbBackgroundTasksStore(databaseConfig.Bind);
        }

        public static BackgroundTaskBuilder AddDocumentDbBackgroundTasksStore(this BackgroundTaskBuilder builder, Action<DocumentDbOptions> configureDatabase = null)
        {
            Bootstrap.EnsureInitialized();

            var services = builder.Services;

            services.TryAddSingleton<IServerTimestampService, LocalServerTimestampService>();
            services.AddScoped<IDocumentDbRepository<BackgroundTaskDocument>, DocumentDbRepository<BackgroundTaskDocument>>();
            services.Replace(ServiceDescriptor.Singleton<IBackgroundTaskStore, DocumentBackgroundTaskStore>());
            services.Configure(configureDatabase);

            services.AddMetrics();
            var serviceProvider = services.BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<BackgroundTaskOptions>>();

            var dbOptions = new DocumentDbOptions();
            configureDatabase?.Invoke(dbOptions);

            MigrateToLatest(options.Value, dbOptions);

            return builder;
        }

        private static void MigrateToLatest(BackgroundTaskOptions taskOptions, DocumentDbOptions dbOptions)
        {
            var runner = new DocumentDbMigrationRunner(dbOptions);

            if (taskOptions.Store.CreateIfNotExists)
            {
                runner.CreateDatabaseIfNotExistsAsync().GetAwaiter().GetResult();
            }

            if (taskOptions.Store.MigrateOnStartup)
            {
                var schema = new CreateBackgroundTasksSchema(runner.Client, dbOptions);
                schema.Up().GetAwaiter().GetResult();
            }
        }
    }
}
