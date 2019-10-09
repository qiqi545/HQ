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

			builder.Services.AddDatabaseConnection<BackgroundTaskBuilder, SqliteConnectionFactory>(connectionString,scope, extensions);
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
				runner.MigrateUp(typeof(CreateBackgroundTasksSchema).Assembly,
					typeof(CreateBackgroundTasksSchema).Namespace);
			}
		}
	}
}