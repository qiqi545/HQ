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
using HQ.Common.Models;
using HQ.Data.SessionManagement;
using HQ.Data.SessionManagement.SqlServer;
using HQ.Data.Sql.SqlServer;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Identity.Stores.Sql.SqlServer
{
    public static class Add
    {
        public static IdentityBuilder AddSqlServerIdentityStore<TUser, TRole, TTenant>(
            this IdentityBuilder identityBuilder,
            IConfiguration config, string connectionString, ConnectionScope scope = ConnectionScope.ByRequest)
            where TUser : IdentityUserExtended<string>
            where TRole : IdentityRoleExtended<string>
            where TTenant : IdentityTenant<string>
        {
            return AddSqlServerIdentityStore<string, TUser, TRole, TTenant>(identityBuilder, connectionString, scope);
        }

        public static IdentityBuilder AddSqlServerIdentityStore<TKey, TUser, TRole, TTenant>(
            this IdentityBuilder identityBuilder, IConfiguration config, string connectionString,
            ConnectionScope scope = ConnectionScope.ByRequest)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
        {
            return AddSqlServerIdentityStore<TKey, TUser, TRole, TTenant>(identityBuilder, connectionString, scope);
        }

        public static IdentityBuilder AddSqlServerIdentityStore<TKey, TUser, TRole, TTenant>(
            IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope,
            IConfiguration identityConfig, IConfiguration SqlServerConfig)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
        {
            identityBuilder.Services.Configure<IdentityOptionsExtended>(identityConfig);
            identityBuilder.Services.Configure<SqlServerOptions>(SqlServerConfig);

            return AddSqlServerIdentityStore<TKey, TUser, TRole, TTenant>(identityBuilder, connectionString, scope,
                identityConfig.Bind, SqlServerConfig.Bind);
        }

        public static IdentityBuilder AddSqlServerIdentityStore<TKey, TUser, TRole, TTenant>(
            this IdentityBuilder identityBuilder, string connectionString,
            ConnectionScope scope = ConnectionScope.ByRequest,
            Action<IdentityOptionsExtended> configureIdentity = null,
            Action<SqlServerOptions> configureSqlServer = null)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
        {
            identityBuilder.Services.AddSingleton<ITypeRegistry, TypeRegistry>();

            var dialect = new SqlServerDialect();

            var identityOptions = new IdentityOptionsExtended();
            configureIdentity?.Invoke(identityOptions);

            var options = new SqlServerOptions();
            configureSqlServer?.Invoke(options);

            MigrateToLatest<TKey>(connectionString, identityOptions, options);

            identityBuilder.AddSqlStores<SqlServerConnectionFactory, TKey, TUser, TRole, TTenant>(connectionString,
                scope,
                OnCommand<TKey>(), OnConnection);

            identityBuilder.Services.AddSingleton(dialect);

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
                if (c is SqlCommand command)
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
