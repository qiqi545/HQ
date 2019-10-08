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
using HQ.Data.Contracts.Schema;
using HQ.Data.Contracts.Schema.Configuration;
using HQ.Data.Contracts.Schema.Models;
using HQ.Extensions.Metrics;
using HQ.Integration.DocumentDb.SessionManagement;
using HQ.Integration.DocumentDb.Sql.DbProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Constants = HQ.Common.Constants;

namespace HQ.Integration.DocumentDb.Schema
{
	public static class Add
	{
		public static SchemaBuilder AddDocumentDbSchemaStores(this SchemaBuilder builder,
			IConfiguration configuration = null)
		{
			return builder.AddDocumentDbSchemaStores(configuration.Bind);
		}

		public static SchemaBuilder AddDocumentDbSchemaStores(this SchemaBuilder builder, string connectionString)
		{
			return builder.AddDocumentDbSchemaStores(o => { DefaultDbOptions(connectionString, o); });
		}

		private static void DefaultDbOptions(string connectionString, DocumentDbOptions o)
		{
			var connectionStringBuilder = new DocumentDbConnectionStringBuilder(connectionString);

			o.AccountKey = connectionStringBuilder.AccountKey;
			o.AccountEndpoint = connectionStringBuilder.AccountEndpoint;
			o.CollectionId = connectionStringBuilder.DefaultCollection ?? Constants.Schemas.DefaultCollection;
			o.DatabaseId = connectionStringBuilder.Database ?? "Default";

			o.SharedCollection = true; // SchemaVersionDocument, ApplicationVersionDocument
			o.PartitionKeyPaths = new[] {"/id"};
		}

		public static SchemaBuilder AddDocumentDbSchemaStores(this SchemaBuilder builder,
			Action<DocumentDbOptions> configureAction = null)
		{
			Bootstrap.SetDefaultJsonSettings();

			const string slot = Constants.ConnectionSlots.Schema;

			if (configureAction != null)
				builder.Services.Configure(slot, configureAction);

			builder.Services.AddLocalTimestamps();
			builder.Services.AddMetrics();
			builder.Services.AddSingleton<IApplicationVersionStore, DocumentApplicationVersionStore>();
			builder.Services.AddSingleton<ISchemaVersionStore, DocumentSchemaVersionStore>();

			var serviceProvider = builder.Services.BuildServiceProvider();
			var options = serviceProvider.GetRequiredService<IOptions<SchemaOptions>>();

			var dbOptions = new DocumentDbOptions();
			configureAction?.Invoke(dbOptions);

			MigrateToLatest(options.Value, dbOptions);

			return builder;
		}

		private static void MigrateToLatest(SchemaOptions schemaOptions, DocumentDbOptions dbOptions)
		{
			var runner = new DocumentDbMigrationRunner(dbOptions);

			if (schemaOptions.Store.CreateIfNotExists)
			{
				runner.CreateDatabaseIfNotExistsAsync().GetAwaiter().GetResult();
			}

			if (schemaOptions.Store.MigrateOnStartup)
			{
				runner.CreateCollectionIfNotExistsAsync().GetAwaiter().GetResult();
			}
		}
	}
}