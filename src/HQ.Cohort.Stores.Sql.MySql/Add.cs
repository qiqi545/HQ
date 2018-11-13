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
using FluentMigrator.Runner;
using HQ.Connect;
using HQ.Connect.MySql;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Cohort.Stores.Sql.MySql
{
    public static class Add
    {
        public static IdentityBuilder AddMySqlIdentityStores<TUser, TRole>(this IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            Action<IdentityOptions> setupAction = null)
            where TUser : IdentityUser<string>
            where TRole : IdentityRole<string>
        {
            MigrateToLatest<string>(connectionString);

            return identityBuilder.AddSqlStores<MySqlConnectionFactory, string, TUser, TRole>(connectionString, scope);
        }

        public static IdentityBuilder AddMySqlIdentityStores<TKey, TUser, TRole>(this IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            Action<IdentityOptions> setupAction = null)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUser<TKey>
            where TRole : IdentityRole<TKey>
        {
            MigrateToLatest<string>(connectionString);

            return identityBuilder.AddSqlStores<MySqlConnectionFactory, TKey, TUser, TRole>(connectionString, scope);
        }

        private static void MigrateToLatest<TKey>(string connectionString) where TKey : IEquatable<TKey>
        {
            var container = new ServiceCollection()
                .AddSingleton(new ZeroMigrationContext {Database = SupportedDatabases.MySql})
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    builder => builder
                        .AddSQLite()
                        .WithGlobalConnectionString(connectionString)
                        .ScanIn(typeof(CreateIdentitySchema).Assembly).For.Migrations())
                .BuildServiceProvider();

            var runner = container.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }
    }
}
