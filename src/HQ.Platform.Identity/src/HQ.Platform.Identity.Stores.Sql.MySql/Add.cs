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
using HQ.Common.Models;
using HQ.Data.Contracts.Queryable;
using HQ.Data.SessionManagement;
using HQ.Data.SessionManagement.MySql;
using HQ.Data.Sql.Dapper;
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.MySql;
using HQ.Data.Sql.Queries;
using HQ.Extensions.Metrics;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace HQ.Platform.Identity.Stores.Sql.MySql
{
    public static class Add
    {
        public static IdentityBuilder AddMySqlIdentityStore<TUser, TRole, TTenant>(
            this IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            IConfiguration identityConfig = null, IConfiguration databaseConfig = null)
            where TUser : IdentityUserExtended<string>
            where TRole : IdentityRoleExtended<string>
            where TTenant : IdentityTenant
        {
            return identityBuilder.AddMySqlIdentityStore<string, TUser, TRole, TTenant>(connectionString, scope,
                identityConfig, databaseConfig);
        }

        public static IdentityBuilder AddMySqlIdentityStore<TKey, TUser, TRole, TTenant>(
            this IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            IConfiguration identityConfig = null,
            IConfiguration databaseConfig = null)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
        {
            var configureIdentity =
                identityConfig != null ? identityConfig.Bind : (Action<IdentityOptionsExtended>)null;

            var configureDatabase =
                databaseConfig != null ? databaseConfig.Bind : (Action<MySqlOptions>)null;

            return AddMySqlIdentityStore<TKey, TUser, TRole, TTenant>(identityBuilder, connectionString, scope,
                configureIdentity,
                configureDatabase);
        }
        
        public static IdentityBuilder AddMySqlIdentityStore<TKey, TUser, TRole, TTenant>(
            this IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            Action<IdentityOptionsExtended> configureIdentity = null,
            Action<MySqlOptions> configureDatabase = null)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
        {
            identityBuilder.Services.AddSingleton<ITypeRegistry, TypeRegistry>();

            var dialect = new MySqlDialect();

            var serviceProvider = identityBuilder.Services.BuildServiceProvider();

            var identityOptions = serviceProvider.GetService<IOptions<IdentityOptionsExtended>>()?.Value ??
                                  new IdentityOptionsExtended();
            configureIdentity?.Invoke(identityOptions);

            var options = serviceProvider.GetService<IOptions<MySqlOptions>>()?.Value ?? new MySqlOptions();
            configureDatabase?.Invoke(options);

            identityBuilder.AddSqlStores<MySqlConnectionFactory, TKey, TUser, TRole, TTenant>(connectionString, scope,
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
                    default:
                        return s;
                }
            };

            DescriptorColumnMapper.AddTypeMap<TUser>(StringComparer.Ordinal);
            DescriptorColumnMapper.AddTypeMap<TRole>(StringComparer.Ordinal);
            DescriptorColumnMapper.AddTypeMap<TTenant>(StringComparer.Ordinal);

            identityBuilder.Services.AddMetrics();
            identityBuilder.Services.AddSingleton(dialect);

            identityBuilder.Services.AddSingleton<IQueryableProvider<TUser>, NoQueryableProvider<TUser>>();
            identityBuilder.Services.AddSingleton<IQueryableProvider<TRole>, NoQueryableProvider<TRole>>();
            identityBuilder.Services.AddSingleton<IQueryableProvider<TTenant>, NoQueryableProvider<TTenant>>();

            MigrateToLatest<TKey>(connectionString, identityOptions, options);

            return identityBuilder;
        }

        private static void OnConnection(IDbConnection c, IServiceProvider r)
        {
            if (c is MySqlConnection connection)
            {
            }
        }

        private static Action<IDbCommand, Type, IServiceProvider> OnCommand<TKey>() where TKey : IEquatable<TKey>
        {
            return (c, t, r) =>
            {
                if (c is MySqlCommand command)
                {
                }
            };
        }

        private static void MigrateToLatest<TKey>(string connectionString, IdentityOptionsExtended identityOptions,
            MySqlOptions options) where TKey : IEquatable<TKey>
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
