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
using System.Threading;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using HQ.Data.Sql.Migration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Integration.SqlServer.Identity
{
    public class MigrationRunner
    {
        private readonly string _connectionString;
        private readonly SqlServerOptions _options;

        public MigrationRunner(string connectionString, SqlServerOptions options)
        {
            _connectionString = connectionString;
            _options = options;
        }

        public async Task CreateDatabaseIfNotExistsAsync(CancellationToken cancellationToken)
        {
            var csb = new SqlConnectionStringBuilder(_connectionString);
            if (csb.DataSource.Equals("(localdb)\\mssqllocaldb", StringComparison.OrdinalIgnoreCase))
            {
                var dbFilePath = $"%USERPROFILE%\\{csb.InitialCatalog}.mdf";
                var filePath = Environment.ExpandEnvironmentVariables(dbFilePath);
                if (!File.Exists(filePath))
                {
                    using (var connection =
                        new SqlConnection(
                            @"Data Source=(localdb)\mssqllocaldb;Initial Catalog=master;Integrated Security=True"))
                    {
                        connection.Open();

                        {
                            var command = connection.CreateCommand();
                            command.CommandText =
                                $"IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{csb.InitialCatalog}') EXEC SP_DETACH_DB N'{csb.InitialCatalog}'";
                            await command.ExecuteNonQueryAsync(cancellationToken);
                        }

                        {
                            var command = connection.CreateCommand();
                            command.CommandText =
                                $"CREATE DATABASE [{csb.InitialCatalog}] ON (NAME = N'{csb.InitialCatalog}', FILENAME = '{filePath}')";
                            await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                    }
                }
            }
        }

        public void MigrateUp(CancellationToken cancellationToken)
        {
            var container = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    builder => builder
                        .AddSqlServer()
                        .WithGlobalConnectionString(_connectionString)
                        .ScanIn(typeof(CreateIdentitySchema).Assembly).For.Migrations())
                .BuildServiceProvider();

            var runner = container.GetRequiredService<IMigrationRunner>();
            if (runner is FluentMigrator.Runner.MigrationRunner defaultRunner &&
                defaultRunner.MigrationLoader is DefaultMigrationInformationLoader defaultLoader)
            {
                var source = container.GetRequiredService<IFilteringMigrationSource>();
                defaultRunner.MigrationLoader =
                    new NamespaceMigrationInformationLoader(typeof(MigrationRunner).Namespace, source, defaultLoader);
            }

            ;
            runner.MigrateUp();
        }
    }
}
