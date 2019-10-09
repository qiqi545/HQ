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
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Queries;
using HQ.Extensions.DependencyInjection.AspNetCore;
using HQ.Extensions.Logging;
using HQ.Extensions.Metrics;
using HQ.Extensions.Options;
using HQ.Extensions.Scheduling;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Models;
using HQ.Integration.SqlServer.SessionManagement;
using HQ.Integration.SqlServer.Sql;
using HQ.Integration.SqlServer.Sql.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace HQ.Integration.SqlServer.Scheduling
{
	public static class Add
	{
		public static BackgroundTaskBuilder AddSqlServerBackgroundTasksStore(this BackgroundTaskBuilder builder,
			string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
			IConfiguration databaseConfig = null)
		{
			return builder.AddSqlServerBackgroundTasksStore(connectionString, scope, databaseConfig.FastBind);
		}

		public static BackgroundTaskBuilder AddSqlServerBackgroundTasksStore(this BackgroundTaskBuilder builder,
			string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
			Action<SqlServerOptions> configureDatabase = null)
		{
			var services = builder.Services;

			if (scope == ConnectionScope.ByRequest)
				services.AddHttpContextAccessor();

			services.AddLocalTimestamps();
			services.AddSafeLogging();
			services.AddDatabaseConnection<BackgroundTaskBuilder, SqlServerConnectionFactory>(connectionString, scope, new[] {new HttpAccessorExtension()});
			services.Replace(ServiceDescriptor.Singleton<IBackgroundTaskStore, SqlServerBackgroundTaskStore>());

			var dialect = new SqlServerDialect();
			SqlBuilder.Dialect = dialect;

			services.Configure(configureDatabase);

			services.AddMetrics();
			services.TryAddSingleton<ISqlDialect>(dialect);
			services.TryAddSingleton(dialect);

			var serviceProvider = services.BuildServiceProvider();

			var options = serviceProvider.GetRequiredService<IOptions<BackgroundTaskOptions>>();

			MigrateToLatest(connectionString, options.Value);

			return builder;
		}

		private static void MigrateToLatest(string connectionString, BackgroundTaskOptions options)
		{
			var runner = new SqlServerMigrationRunner(connectionString);

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