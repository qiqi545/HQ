#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

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
using Constants = HQ.Common.Constants;

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
	        return builder.AddDocumentDbBackgroundTasksStore(o => { DefaultDbOptions(connectionString, o); });
        }
        private static void DefaultDbOptions(string connectionString, DocumentDbOptions o)
        {
	        var connectionStringBuilder = new DocumentDbConnectionStringBuilder(connectionString);

	        o.AccountKey = connectionStringBuilder.AccountKey;
	        o.AccountEndpoint = connectionStringBuilder.AccountEndpoint;
	        o.CollectionId = connectionStringBuilder.DefaultCollection ?? "BackgroundTasks";
	        o.DatabaseId = connectionStringBuilder.Database ?? "Default";
	        o.SharedCollection = false;
        }

        public static BackgroundTaskBuilder AddDocumentDbBackgroundTasksStore(this BackgroundTaskBuilder builder, Action<DocumentDbOptions> configureAction = null)
        {
            Bootstrap.SetDefaultJsonSettings();

            const string slot = Constants.ConnectionSlots.BackgroundTasks;

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
