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
	    public static IdentityBuilder AddSqlServerIdentityStores<TUser, TRole>(this IdentityBuilder identityBuilder, string connectionString, ConnectionScope scope = ConnectionScope.ByRequest, Action<IdentityOptions> setupAction = null)
		    where TUser : IdentityUser<string>
		    where TRole : IdentityRole<string>
	    {
		    MigrateToLatest<string>(connectionString);

		    identityBuilder.Services.AddSingleton<ISqlDialect>(new SqlServerDialect());
			
			return identityBuilder.AddSqlStores<SqlServerConnectionFactory, string, TUser, TRole>(connectionString, scope);
	    }

		public static IdentityBuilder AddSqlServerIdentityStores<TKey, TUser, TRole>(this IdentityBuilder identityBuilder, string connectionString, ConnectionScope scope = ConnectionScope.ByRequest, Action<IdentityOptions> setupAction = null)
		    where TKey : IEquatable<TKey>
		    where TUser : IdentityUser<TKey>
		    where TRole : IdentityRole<TKey>
		{
			MigrateToLatest<TKey>(connectionString);

			identityBuilder.Services.AddSingleton<ISqlDialect>(new SqlServerDialect());

			return identityBuilder.AddSqlStores<SqlServerConnectionFactory, TKey, TUser, TRole>(connectionString, scope);
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
			    {
					using (var connection = new SqlConnection(@"Data Source=(localdb)\mssqllocaldb;Initial Catalog=master;Integrated Security=True"))
				    {
					    connection.Open();
					    { 
						    var command = connection.CreateCommand();
						    command.CommandText = $"IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{csb.InitialCatalog}') EXEC SP_DETACH_DB N'{csb.InitialCatalog}'";
						    command.ExecuteNonQuery();
						}

					    {
						    var command = connection.CreateCommand();
						    command.CommandText = $"CREATE DATABASE [{csb.InitialCatalog}] ON (NAME = N'{csb.InitialCatalog}', FILENAME = '{filePath}')";
						    command.ExecuteNonQuery();
						}
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
