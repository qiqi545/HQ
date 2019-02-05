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
using HQ.Cadence;
using HQ.Extensions.Identity.Configuration;
using HQ.Extensions.Identity.Models;
using HQ.Extensions.Identity.Stores.Sql.Models;
using HQ.Common.Models;
using HQ.Connect;
using HQ.Connect.Sqlite;
using HQ.Lingo.Descriptor;
using HQ.Lingo.Queries;
using HQ.Lingo.Sqlite;
using HQ.Rosetta.Queryable;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Identity.Stores.Sql.Sqlite
{
    public static class Add
    {
        public static IdentityBuilder AddSqliteIdentityStore<TUser, TRole>(this IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest)
            where TUser : IdentityUserExtended<string>
            where TRole : IdentityRoleExtended<string>
        {
            return AddSqliteIdentityStore<string, TUser, TRole>(identityBuilder, connectionString, null, scope);
        }

        public static IdentityBuilder AddSqliteIdentityStore<TKey, TUser, TRole>(this IdentityBuilder identityBuilder,
            string connectionString,
            IConfiguration sqliteConfig,
            ConnectionScope scope = ConnectionScope.ByRequest)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
        {
            if (sqliteConfig != null) identityBuilder.Services.Configure<SqliteOptions>(sqliteConfig);
            var configureSqlite = sqliteConfig != null ? sqliteConfig.Bind : (Action<SqliteOptions>) null;

            return AddSqliteIdentityStore<TKey, TUser, TRole>(identityBuilder, connectionString, scope, null, configureSqlite);
        }
        
        public static IdentityBuilder AddSqliteIdentityStore<TKey, TUser, TRole>(IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope, 
            IConfiguration identityConfig, IConfiguration sqliteConfig)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
        {
            identityBuilder.Services.Configure<IdentityOptions>(identityConfig);
            identityBuilder.Services.Configure<IdentityOptionsExtended>(identityConfig);
            identityBuilder.Services.Configure<SqliteOptions>(sqliteConfig);

            return AddSqliteIdentityStore<TKey, TUser, TRole>(identityBuilder, connectionString, scope, identityConfig.Bind, sqliteConfig.Bind);
        }

        public static IdentityBuilder AddSqliteIdentityStore<TKey, TUser, TRole>(this IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            Action<IdentityOptionsExtended> configureIdentity = null,
            Action<SqliteOptions> configureSqlite = null)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
        {
            identityBuilder.Services.AddSingleton<ITypeRegistry, TypeRegistry>();

            var dialect = new SqliteDialect();

            var serviceProvider = identityBuilder.Services.BuildServiceProvider();

            var identityOptions = serviceProvider.GetService<IOptions<IdentityOptionsExtended>>()?.Value ?? new IdentityOptionsExtended();
            configureIdentity?.Invoke(identityOptions);

            var options = serviceProvider.GetService<IOptions<SqliteOptions>>()?.Value ?? new SqliteOptions();
            configureSqlite?.Invoke(options);

            identityBuilder.AddSqlStores<SqliteConnectionFactory, TKey, TUser, TRole>(connectionString, scope, OnCommand<TKey>(), OnConnection);
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
                    default:
                        return s;
                }
            };

            identityBuilder.Services.AddMetrics();
            identityBuilder.Services.AddSingleton(dialect);
            identityBuilder.Services.AddSingleton<IQueryableProvider<TUser>, NoQueryableProvider<TUser>>();
            identityBuilder.Services.AddSingleton<IQueryableProvider<TRole>, NoQueryableProvider<TRole>>();

            lock (identityBuilder)
            {
                MigrateToLatest<TKey>(connectionString, identityOptions, options);
            }

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

        private static void MigrateToLatest<TKey>(string connectionString, IdentityOptionsExtended identityOptions, SqliteOptions options) where TKey : IEquatable<TKey>
        {
            var runner = new MigrationRunner(connectionString, options);

            if (identityOptions.Stores.CreateIfNotExists)
                runner.CreateDatabaseIfNotExistsAsync(CancellationToken.None).Wait();

            if (identityOptions.Stores.MigrateOnStartup)
                runner.MigrateUp(CancellationToken.None);
        }
    }
}
