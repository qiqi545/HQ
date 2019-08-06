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
using HQ.Common;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Batching;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Queries;
using HQ.Extensions.Metrics;
using HQ.Integration.DocumentDb.DbProvider;
using HQ.Integration.DocumentDb.SessionManagement;
using HQ.Integration.DocumentDb.Sql;
using HQ.Platform.Api;
using HQ.Platform.Api.Runtime;
using HQ.Platform.Identity.Stores.Sql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Constants = HQ.Common.Constants;

namespace HQ.Integration.DocumentDb.Runtime
{
	public static class Add
	{
		public static IServiceCollection AddDocumentDbRuntime(
			this IServiceCollection services,
			string connectionString, ConnectionScope scope = ConnectionScope.ByRequest)
		{
			return AddDocumentDbRuntime(services, connectionString, scope, o =>
			{
				var builder = new DocumentDbConnectionStringBuilder(connectionString);
				o.AccountKey = o.AccountKey ?? builder.AccountKey;
				o.AccountEndpoint = o.AccountEndpoint ?? builder.AccountEndpoint;
				o.DatabaseId = o.DatabaseId ?? builder.Database;
				o.CollectionId = o.CollectionId ?? builder.DefaultCollection ?? Constants.Runtime.DefaultCollection;
			});
		}

		public static IServiceCollection AddDocumentDbRuntime(this IServiceCollection services,
			string connectionString, 
			ConnectionScope scope = ConnectionScope.ByRequest,
			Action<DocumentDbOptions> configureDatabase = null)
		{
			services.AddSingleton<ITypeRegistry, TypeRegistry>();

			void ConfigureDatabase(DocumentDbOptions o) { configureDatabase?.Invoke(o); }
			services.Configure<DocumentDbOptions>(ConfigureDatabase);

			var serviceProvider = services.BuildServiceProvider();

			var dbOptions = serviceProvider.GetService<IOptions<DocumentDbOptions>>()?.Value ?? new DocumentDbOptions();
			configureDatabase?.Invoke(dbOptions);

			var dialect = new DocumentDbDialect();
			SqlBuilder.Dialect = dialect;

			services.AddSqlRuntimeStores<DocumentDbConnectionFactory>(connectionString, scope, OnCommand(), OnConnection);
			
			services.AddMetrics();
			services.TryAddSingleton<ISqlDialect>(dialect);
			services.TryAddSingleton(dialect);
			services.TryAddSingleton<IDataBatchOperation<DocumentDbBatchOptions>, DocumentDbBatchDataOperation>();

			var runtimeOptions = serviceProvider.GetRequiredService<IOptions<RuntimeOptions>>().Value;

			MigrateToLatest(runtimeOptions, dbOptions);

			return services;
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
