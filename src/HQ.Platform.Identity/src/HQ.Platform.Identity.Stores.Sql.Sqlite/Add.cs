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
using System.Threading;
using HQ.Common;
using HQ.Data.Contracts.Queryable;
using HQ.Data.SessionManagement;
using HQ.Data.SessionManagement.Sqlite;
using HQ.Data.Sql.Dapper;
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.Queries;
using HQ.Data.Sql.Sqlite;
using HQ.Extensions.Metrics;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Identity.Stores.Sql.Sqlite
{
    public static class Add
    {
        public static IdentityBuilder AddSqliteIdentityStore<TUser, TRole, TTenant, TApplication>(
            this IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            IConfiguration databaseConfig = null)
            where TUser : IdentityUserExtended<string>
            where TRole : IdentityRoleExtended<string>
            where TTenant : IdentityTenant<string>
            where TApplication : IdentityApplication<string>
        {
            return identityBuilder.AddSqliteIdentityStore<string, TUser, TRole, TTenant, TApplication>(connectionString, scope,
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
                databaseConfig != null ? databaseConfig.Bind : (Action<SqliteOptions>) null;

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
            identityBuilder.Services.AddSingleton<ITypeRegistry, TypeRegistry>();

            var dialect = new SqliteDialect();

            var builder = new SqliteConnectionStringBuilder(connectionString);

            void ConfigureDatabase(SqliteOptions o)
            {
                configureDatabase?.Invoke(o);
            }

            identityBuilder.Services.Configure<SqliteOptions>(ConfigureDatabase);

            var serviceProvider = identityBuilder.Services.BuildServiceProvider();

            var options = serviceProvider.GetService<IOptions<SqliteOptions>>()?.Value ?? new SqliteOptions();
            configureDatabase?.Invoke(options);

            identityBuilder.AddSqlStores<SqliteConnectionFactory, TKey, TUser, TRole, TTenant>(connectionString, scope,
                OnCommand<TKey>(), OnConnection);
            identityBuilder.Services.AddSingleton(dialect);

            SqlBuilder.Dialect = dialect;

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

            identityBuilder.Services.AddMetrics();
            identityBuilder.Services.AddSingleton(dialect);

            identityBuilder.Services.AddSingleton<IQueryableProvider<TUser>, NoQueryableProvider<TUser>>();
            identityBuilder.Services.AddSingleton<IQueryableProvider<TRole>, NoQueryableProvider<TRole>>();
            identityBuilder.Services.AddSingleton<IQueryableProvider<TTenant>, NoQueryableProvider<TTenant>>();
            identityBuilder.Services.AddSingleton<IQueryableProvider<TApplication>, NoQueryableProvider<TApplication>>();

            var identityOptions = serviceProvider.GetRequiredService<IOptions<IdentityOptionsExtended>>().Value;

            MigrateToLatest<TKey>(connectionString, identityOptions, options);

            return identityBuilder;
        }

        private static void OnConnection(IDbConnection c, IServiceProvider r)
        {
            if (c is SqliteConnection connection)
            {
            }
        }

        private static Action<IDbCommand, Type, IServiceProvider> OnCommand<TKey>() where TKey : IEquatable<TKey>
        {
            return (c, t, r) =>
            {
                if (c is SqliteCommand command)
                {
                }
            };
        }

        private static void MigrateToLatest<TKey>(string connectionString, IdentityOptionsExtended identityOptions,
            SqliteOptions options) where TKey : IEquatable<TKey>
        {
            var runner = new MigrationRunner(connectionString, options);

            if (identityOptions.Stores.CreateIfNotExists)
            {
                runner.CreateDatabaseIfNotExistsAsync(CancellationToken.None).Wait();
            }

            if (identityOptions.Stores.MigrateOnStartup)
            {
                runner.MigrateUp(CancellationToken.None);
            }
        }
    }
}
