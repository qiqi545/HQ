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

using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Data.Sql.Migration
{
	public abstract class MigrationRunner
	{
		protected readonly string ConnectionString;

		protected MigrationRunner(string connectionString) => ConnectionString = connectionString;

		public abstract void CreateDatabaseIfNotExists();

		public abstract void ConfigureMigrator(IMigrationRunnerBuilder builder);

		public void MigrateUp(Assembly assembly, string ns)
		{
			var container = new ServiceCollection()
				.AddFluentMigratorCore()
				.ConfigureRunner(
					builder =>
					{
						ConfigureMigrator(builder);
						builder
							.WithGlobalConnectionString(ConnectionString)
							.ScanIn(assembly).For.Migrations();
					})
				.BuildServiceProvider();

			var runner = container.GetRequiredService<IMigrationRunner>();
			if (runner is FluentMigrator.Runner.MigrationRunner defaultRunner &&
			    defaultRunner.MigrationLoader is DefaultMigrationInformationLoader defaultLoader)
			{
				var source = container.GetRequiredService<IFilteringMigrationSource>();
				defaultRunner.MigrationLoader = new NamespaceMigrationInformationLoader(ns, source, defaultLoader);
			}

			runner.MigrateUp();
		}
	}
}