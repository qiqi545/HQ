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
using HQ.Data.SessionManagement;
using HQ.Data.SessionManagement.MySql;
using HQ.Data.Sql.MySql;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace HQ.Platform.Identity.Stores.Sql.MySql
{
    public static class Add
    {
        public static IdentityBuilder AddMySqlIdentityStore<TUser, TRole, TTenant>(this IdentityBuilder identityBuilder,
            IConfiguration config, string connectionString, ConnectionScope scope = ConnectionScope.ByRequest)
            where TUser : IdentityUserExtended<string>
            where TRole : IdentityRoleExtended<string>
            where TTenant : IdentityTenant<string>
        {
            return AddMySqlIdentityStore<string, TUser, TRole, TTenant>(identityBuilder, connectionString, scope);
        }

        public static IdentityBuilder AddMySqlIdentityStore<TKey, TUser, TRole, TTenant>(
            this IdentityBuilder identityBuilder,
            IConfiguration config, string connectionString, ConnectionScope scope = ConnectionScope.ByRequest)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
        {
            return AddMySqlIdentityStore<TKey, TUser, TRole, TTenant>(identityBuilder, connectionString, scope);
        }

        public static IdentityBuilder AddMySqlIdentityStore<TKey, TUser, TRole, TTenant>(
            IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope,
            IConfiguration identityConfig, IConfiguration mySqlConfig)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
        {
            identityBuilder.Services.Configure<IdentityOptionsExtended>(identityConfig);
            identityBuilder.Services.Configure<MySqlOptions>(mySqlConfig);

            return AddMySqlIdentityStore<TKey, TUser, TRole, TTenant>(identityBuilder, connectionString, scope,
                identityConfig.Bind, mySqlConfig.Bind);
        }

        public static IdentityBuilder AddMySqlIdentityStore<TKey, TUser, TRole, TTenant>(
            this IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            Action<IdentityOptionsExtended> configureIdentity = null, Action<MySqlOptions> configureMySql = null)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
        {
            identityBuilder.Services.AddSingleton<ITypeRegistry, TypeRegistry>();

            var dialect = new MySqlDialect();

            var identityOptions = new IdentityOptionsExtended();
            configureIdentity?.Invoke(identityOptions);

            var mySqlOptions = new MySqlOptions();
            configureMySql?.Invoke(mySqlOptions);

            MigrateToLatest<TKey>(connectionString, identityOptions, mySqlOptions);

            identityBuilder.AddSqlStores<MySqlConnectionFactory, TKey, TUser, TRole, TTenant>(connectionString, scope,
                OnCommand<TKey>(), OnConnection);

            identityBuilder.Services.AddSingleton(dialect);

            return identityBuilder;
        }

        private static void OnConnection(IDbConnection c, IServiceProvider r)
        {
            if (c is MySqlConnection connection)
            {
            }
        }

        private static Action<IDbCommand, Type, IServiceProvider> OnCommand<TKey>()
            where TKey : IEquatable<TKey>
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
