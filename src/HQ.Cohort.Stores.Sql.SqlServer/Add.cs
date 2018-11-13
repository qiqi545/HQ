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
using System.Data.SqlClient;
using System.IO;
using FluentMigrator.Runner;
using HQ.Connect;
using HQ.Connect.SqlServer;
using HQ.Lingo.Dialects;
using HQ.Lingo.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Cohort.Stores.Sql.SqlServer
{
    public static class Add
    {
        public static IdentityBuilder AddSqlServerIdentityStores<TUser, TRole>(this IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            Action<IdentityOptions> setupAction = null)
            where TUser : IdentityUser<string>
            where TRole : IdentityRole<string>
        {
            MigrateToLatest<string>(connectionString);

            identityBuilder.Services.AddSingleton<ISqlDialect>(new SqlServerDialect());

            return identityBuilder.AddSqlStores<SqlServerConnectionFactory, string, TUser, TRole>(connectionString,
                scope);
        }

        public static IdentityBuilder AddSqlServerIdentityStores<TKey, TUser, TRole>(
            this IdentityBuilder identityBuilder, string connectionString,
            ConnectionScope scope = ConnectionScope.ByRequest, Action<IdentityOptions> setupAction = null)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUser<TKey>
            where TRole : IdentityRole<TKey>
        {
            MigrateToLatest<TKey>(connectionString);

            identityBuilder.Services.AddSingleton<ISqlDialect>(new SqlServerDialect());

            return identityBuilder
                .AddSqlStores<SqlServerConnectionFactory, TKey, TUser, TRole>(connectionString, scope);
        }

        private static void MigrateToLatest<TKey>(string connectionString) where TKey : IEquatable<TKey>
        {
            //
            // Create database if it doesn't exist:
            var csb = new SqlConnectionStringBuilder(connectionString);
            if (csb.DataSource.Equals("(localdb)\\mssqllocaldb", StringComparison.OrdinalIgnoreCase))
            {
                var dbFilePath = $"%USERPROFILE%\\{csb.InitialCatalog}.mdf";
                var filePath = Environment.ExpandEnvironmentVariables(dbFilePath);
                if (!File.Exists(filePath))
                    using (var connection =
                        new SqlConnection(
                            @"Data Source=(localdb)\mssqllocaldb;Initial Catalog=master;Integrated Security=True"))
                    {
                        connection.Open();
                        {
                            var command = connection.CreateCommand();
                            command.CommandText =
                                $"IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{csb.InitialCatalog}') EXEC SP_DETACH_DB N'{csb.InitialCatalog}'";
                            command.ExecuteNonQuery();
                        }

                        {
                            var command = connection.CreateCommand();
                            command.CommandText =
                                $"CREATE DATABASE [{csb.InitialCatalog}] ON (NAME = N'{csb.InitialCatalog}', FILENAME = '{filePath}')";
                            command.ExecuteNonQuery();
                        }
                    }
            }

            //
            // Run migration:
            var container = new ServiceCollection()
                .AddSingleton(new ZeroMigrationContext {Database = SupportedDatabases.SqlServer})
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    builder => builder
                        .AddSqlServer()
                        .WithGlobalConnectionString(connectionString)
                        .ScanIn(typeof(CreateIdentitySchema).Assembly).For.Migrations())
                .BuildServiceProvider();

            var runner = container.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }
    }
}
