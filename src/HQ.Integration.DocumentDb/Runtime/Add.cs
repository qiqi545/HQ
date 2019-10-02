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
using System.Data;
using HQ.Data.Contracts.Runtime;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Batching;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Queries;
using HQ.Extensions.Metrics;
using HQ.Extensions.Options;
using HQ.Integration.DocumentDb.SessionManagement;
using HQ.Integration.DocumentDb.Sql;
using HQ.Integration.DocumentDb.Sql.DbProvider;
using HQ.Platform.Api;
using HQ.Platform.Api.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Constants = HQ.Common.Constants;

namespace HQ.Integration.DocumentDb.Runtime
{
	public static class Add
	{
		public static RuntimeBuilder AddDocumentDbRuntimeStores(this RuntimeBuilder builder, string connectionString, ConnectionScope scope, IConfiguration dbConfig)
		{
			return dbConfig == null
				? AddDocumentDbRuntimeStores(builder, connectionString, scope)
				: AddDocumentDbRuntimeStores(builder, connectionString, scope, dbConfig.FastBind);
		}

		public static RuntimeBuilder AddDocumentDbRuntimeStores(this RuntimeBuilder builder, string connectionString, ConnectionScope scope = ConnectionScope.ByRequest)
		{
			return AddDocumentDbRuntimeStores(builder, connectionString, scope, o => { DefaultDbOptions(connectionString, o); });
		}

		private static void DefaultDbOptions(string connectionString, DocumentDbOptions o)
		{
			var connectionStringBuilder = new DocumentDbConnectionStringBuilder(connectionString);
			o.AccountKey ??= connectionStringBuilder.AccountKey;
			o.AccountEndpoint ??= connectionStringBuilder.AccountEndpoint;
			o.DatabaseId ??= connectionStringBuilder.Database;
			o.CollectionId ??= connectionStringBuilder.DefaultCollection ?? Constants.Runtime.DefaultCollection;

			o.SharedCollection = true; // Anything
			o.PartitionKeyPaths = new[] {"id"};
		}

		private static RuntimeBuilder AddDocumentDbRuntimeStores(this RuntimeBuilder builder, string connectionString, ConnectionScope scope, Action<DocumentDbOptions> configureDatabase)
		{
			var slot = Constants.ConnectionSlots.Runtime;

			if(configureDatabase != null)
				builder.Services.Configure(slot, configureDatabase);

			var serviceProvider = builder.Services.BuildServiceProvider();

			var dbOptions = serviceProvider.GetService<IOptions<DocumentDbOptions>>()?.Value ?? new DocumentDbOptions();
			configureDatabase?.Invoke(dbOptions);

			var dialect = new DocumentDbDialect();
			SqlBuilder.Dialect = dialect;

			builder.AddSqlRuntimeStores<DocumentDbConnectionFactory>(connectionString, scope, OnCommand(), OnConnection);

			builder.Services.AddMetrics();
			builder.Services.TryAddSingleton<ISqlDialect>(dialect);
			builder.Services.TryAddSingleton(dialect);
			builder.Services.TryAddSingleton<IDataBatchOperation<DocumentDbBatchOptions>, DocumentDbBatchDataOperation>();

			var runtimeOptions = serviceProvider.GetRequiredService<IOptions<RuntimeOptions>>().Value;

			MigrateToLatest(runtimeOptions, dbOptions);

			return builder;
		}

		private static void OnConnection(IDbConnection c, IServiceProvider r) { }

		private static Action<IDbCommand, Type, IServiceProvider> OnCommand()
		{
			return (c, t, r) => { };
		}

		private static void MigrateToLatest(RuntimeOptions runtimeOptions, DocumentDbOptions dbOptions)
		{
			var runner = new DocumentDbMigrationRunner(dbOptions);

			if (runtimeOptions.CreateIfNotExists)
			{
				runner.CreateDatabaseIfNotExistsAsync().GetAwaiter().GetResult();
			}

			if (runtimeOptions.MigrateOnStartup)
			{
				runner.CreateCollectionIfNotExistsAsync().GetAwaiter().GetResult();
			}
		}
	}
}
