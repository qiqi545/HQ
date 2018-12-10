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
using System.Threading;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Cohort.Stores.Sql.Sqlite
{
    public class MigrationRunner
    {
        private readonly string _connectionString;
        private readonly SqliteOptions _options;

        public MigrationRunner(string connectionString, SqliteOptions options)
        {
            _connectionString = connectionString;
            _options = options;
        }

        public Task CreateDatabaseIfNotExistsAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;            
        }

        public void MigrateUp(CancellationToken cancellationToken)
        {
            var container = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    builder => builder
                        .AddSQLite()
                        .WithGlobalConnectionString(_connectionString)
                        .ScanIn(typeof(CreateIdentitySchema).Assembly).For.Migrations())
                .BuildServiceProvider();

            var runner = container.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }
    }
}
