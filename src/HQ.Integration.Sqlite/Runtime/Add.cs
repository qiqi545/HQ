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
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.Runtime;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Batching;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Queries;
using HQ.Extensions.Metrics;
using HQ.Integration.Sqlite.Identity;
using HQ.Integration.Sqlite.SessionManagement;
using HQ.Integration.Sqlite.Sql;
using HQ.Integration.Sqlite.Sql.Configuration;
using HQ.Platform.Api;
using HQ.Platform.Api.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace HQ.Integration.Sqlite.Runtime
{
	public static class Add
	{
		public static RuntimeBuilder AddSqliteRuntime(
			this RuntimeBuilder builder,
			string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
			IConfiguration databaseConfig = null)
		{
			var configureDatabase = databaseConfig != null ? databaseConfig.Bind : (Action<SqliteOptions>) null;

			return AddSqliteRuntime(builder, connectionString, scope, configureDatabase);
		}

		public static RuntimeBuilder AddSqliteRuntime(this RuntimeBuilder builder,
			string connectionString, 
			ConnectionScope scope = ConnectionScope.ByRequest,
			Action<SqliteOptions> configureDatabase = null)
		{
			builder.Services.AddTypeDiscovery();

			void ConfigureDatabase(SqliteOptions o) { configureDatabase?.Invoke(o); }
			builder.Services.Configure<SqliteOptions>(ConfigureDatabase);

			var serviceProvider = builder.Services.BuildServiceProvider();

			var options = serviceProvider.GetService<IOptions<SqliteOptions>>()?.Value ?? new SqliteOptions();
			configureDatabase?.Invoke(options);

			var dialect = new SqliteDialect();
			SqlBuilder.Dialect = dialect;

			builder.AddSqlRuntimeStores<SqliteConnectionFactory>(connectionString, scope, OnCommand(), OnConnection);

			builder.Services.AddMetrics();
			builder.Services.TryAddSingleton<ISqlDialect>(dialect);
			builder.Services.TryAddSingleton(dialect);
			builder.Services.TryAddSingleton<IDataBatchOperation<SqliteOptions>, SqliteBatchDataOperation>();

			var runtimeOptions = serviceProvider.GetRequiredService<IOptions<RuntimeOptions>>().Value;

			MigrateToLatest(connectionString, runtimeOptions);

			return builder;
		}

		private static void OnConnection(IDbConnection c, IServiceProvider r) { }

		private static Action<IDbCommand, Type, IServiceProvider> OnCommand()
		{
			return (c, t, r) => { };
		}

		private static void MigrateToLatest(string connectionString, RuntimeOptions options)
		{
			var runner = new SqliteMigrationRunner(connectionString);

			if (options.CreateIfNotExists)
			{
				runner.CreateDatabaseIfNotExists();
			}

			if (options.MigrateOnStartup)
			{
				runner.MigrateUp(typeof(CreateIdentitySchema).Assembly, typeof(CreateIdentitySchema).Namespace);
			}
		}
	}
}
