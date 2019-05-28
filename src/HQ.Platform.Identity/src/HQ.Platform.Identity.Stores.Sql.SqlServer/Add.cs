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
using System.Data.SqlClient;
using System.Threading;
using HQ.Common;
using HQ.Data.Contracts.Queryable;
using HQ.Data.SessionManagement;
using HQ.Data.SessionManagement.SqlServer;
using HQ.Data.Sql.Dapper;
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.Queries;
using HQ.Data.Sql.SqlServer;
using HQ.Extensions.Metrics;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Identity.Stores.Sql.SqlServer
{
    public static class Add
    {
        public static IdentityBuilder AddSqlServerIdentityStore<TUser, TRole, TTenant>(
            this IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            IConfiguration databaseConfig = null)
            where TUser : IdentityUserExtended<string>
            where TRole : IdentityRoleExtended<string>
            where TTenant : IdentityTenant
        {
            return identityBuilder.AddSqlServerIdentityStore<string, TUser, TRole, TTenant>(connectionString, scope,
                databaseConfig);
        }

        public static IdentityBuilder AddSqlServerIdentityStore<TKey, TUser, TRole, TTenant>(
            this IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            IConfiguration databaseConfig = null)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
        {
            var configureDatabase =
                databaseConfig != null ? databaseConfig.Bind : (Action<SqlServerOptions>) null;

            return AddSqlServerIdentityStore<TKey, TUser, TRole, TTenant>(identityBuilder, connectionString, scope,
                configureDatabase);
        }

        public static IdentityBuilder AddSqlServerIdentityStore<TKey, TUser, TRole, TTenant>(
            this IdentityBuilder identityBuilder,
            string connectionString,
            ConnectionScope scope = ConnectionScope.ByRequest,
            Action<SqlServerOptions> configureDatabase = null)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
        {
            var services = identityBuilder.Services;
            services.AddSingleton<ITypeRegistry, TypeRegistry>();

            var serviceProvider = services.BuildServiceProvider();

            var builder = new SqlConnectionStringBuilder(connectionString);

            void ConfigureAction(SqlServerOptions o)
            {
                configureDatabase?.Invoke(o);
            }

            identityBuilder.Services.Configure<SqlServerOptions>(ConfigureAction);

            var dialect = new SqlServerDialect();
            identityBuilder.AddSqlStores<SqlServerConnectionFactory, TKey, TUser, TRole, TTenant>(connectionString,
                scope, OnCommand<TKey>(), OnConnection);

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

            services.AddMetrics();
            services.AddSingleton(dialect);

            identityBuilder.Services.AddSingleton<IQueryableProvider<TUser>, NoQueryableProvider<TUser>>();
            identityBuilder.Services.AddSingleton<IQueryableProvider<TRole>, NoQueryableProvider<TRole>>();
            identityBuilder.Services.AddSingleton<IQueryableProvider<TTenant>, NoQueryableProvider<TTenant>>();

            var options = new SqlServerOptions();
            ConfigureAction(options);

            var identityOptions = serviceProvider.GetRequiredService<IOptions<IdentityOptionsExtended>>().Value;

            MigrateToLatest<TKey>(connectionString, identityOptions, options);

            return identityBuilder;
        }

        private static void OnConnection(IDbConnection c, IServiceProvider r)
        {
            if (c is SqlConnection connection)
            {
            }
        }

        private static Action<IDbCommand, Type, IServiceProvider> OnCommand<TKey>()
            where TKey : IEquatable<TKey>
        {
            return (c, t, r) =>
            {
                if (c is SqlServerOptions command)
                {
                }
            };
        }

        private static void MigrateToLatest<TKey>(string connectionString, IdentityOptionsExtended identityOptions,
            SqlServerOptions options) where TKey : IEquatable<TKey>
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
