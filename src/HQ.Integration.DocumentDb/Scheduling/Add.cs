using System;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Extensions.Metrics;
using HQ.Extensions.Scheduling;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Models;
using HQ.Integration.DocumentDb.SessionManagement;
using HQ.Integration.DocumentDb.Sql.DbProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace HQ.Integration.DocumentDb.Scheduling
{
    public static class Add
    {
        public static BackgroundTaskBuilder AddDocumentDbBackgroundTasksStore(this BackgroundTaskBuilder builder, IConfiguration configuration = null)
        {
            return builder.AddDocumentDbBackgroundTasksStore(configuration.Bind);
        }

        public static BackgroundTaskBuilder AddDocumentDbBackgroundTasksStore(this BackgroundTaskBuilder builder, string connectionString)
        {
	        return builder.AddDocumentDbBackgroundTasksStore(o =>
	        {
		        var connectionStringBuilder = new DocumentDbConnectionStringBuilder(connectionString);

		        o.AccountKey = connectionStringBuilder.AccountKey;
		        o.AccountEndpoint = connectionStringBuilder.AccountEndpoint;
		        o.CollectionId = connectionStringBuilder.DefaultCollection ?? "BackgroundTasks";
		        o.DatabaseId = connectionStringBuilder.Database ?? "Default";
				o.SharedCollection = false;
	        });
        }

		public static BackgroundTaskBuilder AddDocumentDbBackgroundTasksStore(this BackgroundTaskBuilder builder, Action<DocumentDbOptions> configureAction = null)
        {
            Bootstrap.SetDefaultJsonSettings();

            const string slot = "BackgroundTasks";

            if (configureAction != null)
	            builder.Services.Configure(slot, configureAction);

            builder.Services.AddLocalTimestamps();
			builder.Services.AddMetrics();
			builder.Services.AddSingleton<IDocumentDbRepository<BackgroundTaskDocument>>(r => new DocumentDbRepository<BackgroundTaskDocument>(slot, r.GetRequiredService<IOptionsMonitor<DocumentDbOptions>>()));
			builder.Services.Replace(ServiceDescriptor.Singleton<IBackgroundTaskStore, DocumentBackgroundTaskStore>());
			
            var serviceProvider = builder.Services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<BackgroundTaskOptions>>();

            var dbOptions = new DocumentDbOptions();
            configureAction?.Invoke(dbOptions);

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
	            runner.CreateCollectionIfNotExistsAsync().GetAwaiter().GetResult();
            }
        }
    }
}
