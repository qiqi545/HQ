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
using HQ.Data.Contracts.Queryable;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Batching;
using HQ.Data.Sql.Dapper;
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Queries;
using HQ.Extensions.Metrics;
using HQ.Integration.Sqlite.SessionManagement;
using HQ.Integration.Sqlite.Sql;
using HQ.Integration.Sqlite.Sql.Configuration;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Models;
using HQ.Platform.Identity.Stores.Sql;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ActiveOptions;

namespace HQ.Integration.Sqlite.Identity
{
	public static class Add
	{
		public static IdentityBuilder AddSqliteIdentityStore(
			this IdentityBuilder identityBuilder,
			string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
			IConfiguration databaseConfig = null)
		{
			return identityBuilder
				.AddSqliteIdentityStore<string, IdentityUserExtended, IdentityRoleExtended, IdentityTenant,
					IdentityApplication>(connectionString, scope,
					databaseConfig);
		}

		public static IdentityBuilder AddSqliteIdentityStore<TUser, TRole, TTenant, TApplication>(
			this IdentityBuilder identityBuilder,
			string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
			IConfiguration databaseConfig = null)
			where TUser : IdentityUserExtended<string>
			where TRole : IdentityRoleExtended<string>
			where TTenant : IdentityTenant<string>
			where TApplication : IdentityApplication<string>
		{
			return identityBuilder.AddSqliteIdentityStore<string, TUser, TRole, TTenant, TApplication>(connectionString,
				scope,
				databaseConfig);
		}

		public static IdentityBuilder AddSqliteIdentityStore<TKey, TUser, TRole, TTenant, TApplication>(
			this IdentityBuilder identityBuilder,
			string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
			IConfiguration databaseConfig = null)
			where TKey : IEquatable<TKey>
			where TUser : IdentityUserExtended<TKey>
			where TRole : IdentityRoleExtended<TKey>
			where TTenant : IdentityTenant<TKey>
			where TApplication : IdentityApplication<TKey>
		{
			var configureDatabase =
				databaseConfig != null ? databaseConfig.FastBind : (Action<SqliteOptions>) null;

			return AddSqliteIdentityStore<TKey, TUser, TRole, TTenant, TApplication>(identityBuilder,
				connectionString, scope, configureDatabase);
		}

		public static IdentityBuilder AddSqliteIdentityStore<TKey, TUser, TRole, TTenant, TApplication>(
			this IdentityBuilder identityBuilder,
			string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
			Action<SqliteOptions> configureDatabase = null)
			where TKey : IEquatable<TKey>
			where TUser : IdentityUserExtended<TKey>
			where TRole : IdentityRoleExtended<TKey>
			where TTenant : IdentityTenant<TKey>
			where TApplication : IdentityApplication<TKey>
		{
			var services = identityBuilder.Services;

			services.AddSingleton<ITypeRegistry, TypeRegistry>();

			var builder = new SqliteConnectionStringBuilder(connectionString);

			void ConfigureDatabase(SqliteOptions o) { configureDatabase?.Invoke(o); }
			services.Configure<SqliteOptions>(ConfigureDatabase);

			var serviceProvider = services.BuildServiceProvider();

			var options = serviceProvider.GetService<IOptions<SqliteOptions>>()?.Value ?? new SqliteOptions();
			configureDatabase?.Invoke(options);

			var dialect = new SqliteDialect();
			SqlBuilder.Dialect = dialect;

			identityBuilder.AddSqlIdentityStores<SqliteConnectionFactory, TKey, TUser, TRole, TTenant, TApplication>(
				connectionString, scope, OnCommand(), OnConnection);

			SimpleDataDescriptor.TableNameConvention = s =>
			{
				switch (s)
				{
					case nameof(IdentityRoleExtended):
						return "AspNetRoles";
					case nameof(IdentityUserExtended):
						return "AspNetUsers";
					case nameof(IdentityTenant):
						return "AspNetTenants";
					case nameof(IdentityApplication):
						return "AspNetApplications";
					default:
						return s;
				}
			};

			DescriptorColumnMapper.AddTypeMap<TUser>(StringComparer.Ordinal);
			DescriptorColumnMapper.AddTypeMap<TRole>(StringComparer.Ordinal);
			DescriptorColumnMapper.AddTypeMap<TTenant>(StringComparer.Ordinal);
			DescriptorColumnMapper.AddTypeMap<TApplication>(StringComparer.Ordinal);

			services.AddMetrics();
			services.TryAddSingleton<ISqlDialect>(dialect);
			services.TryAddSingleton(dialect);

			services.AddSingleton<IQueryableProvider<TUser>, NoQueryableProvider<TUser>>();
			services.AddSingleton<IQueryableProvider<TRole>, NoQueryableProvider<TRole>>();
			services.AddSingleton<IQueryableProvider<TTenant>, NoQueryableProvider<TTenant>>();
			services.AddSingleton<IQueryableProvider<TApplication>, NoQueryableProvider<TApplication>>();

			services.TryAddSingleton<IDataBatchOperation<SqliteOptions>, SqliteBatchDataOperation>();

			var identityOptions = serviceProvider.GetRequiredService<IOptions<IdentityOptionsExtended>>().Value;

			MigrateToLatest(connectionString, identityOptions);

			return identityBuilder;
		}

		private static void OnConnection(IDbConnection c, IServiceProvider r)
		{
			if (c is SqliteConnection connection)
			{
			}
		}

		private static Action<IDbCommand, Type, IServiceProvider> OnCommand()
		{
			return (c, t, r) =>
			{
				if (c is SqliteCommand command)
				{
				}
			};
		}

		private static void MigrateToLatest(string connectionString, IdentityOptionsExtended identityOptions)
		{
			var runner = new SqliteMigrationRunner(connectionString);

			if (identityOptions.Stores.CreateIfNotExists)
			{
				runner.CreateDatabaseIfNotExists();
			}

			if (identityOptions.Stores.MigrateOnStartup)
			{
				runner.MigrateUp(typeof(CreateIdentitySchema).Assembly, typeof(CreateIdentitySchema).Namespace);
			}
		}
	}
}