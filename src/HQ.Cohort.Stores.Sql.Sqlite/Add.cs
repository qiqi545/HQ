using System;
using FluentMigrator.Runner;
using HQ.Connect;
using HQ.Connect.Sqlite;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Cohort.Stores.Sql.Sqlite
{
    public static class Add
    {
	    public static IdentityBuilder AddSqliteIdentityStores<TUser, TRole>(this IdentityBuilder identityBuilder, string connectionString, ConnectionScope scope = ConnectionScope.ByRequest, Action<IdentityOptions> setupAction = null)
		    where TUser : IdentityUser<string>
		    where TRole : IdentityRole<string>
	    {
			MigrateToLatest<string>(connectionString);

			return identityBuilder.AddSqlStores<SqliteConnectionFactory, string, TUser, TRole>(connectionString, scope);
	    }

	    public static IdentityBuilder AddSqliteIdentityStores<TKey, TUser, TRole>(this IdentityBuilder identityBuilder, string connectionString, ConnectionScope scope = ConnectionScope.ByRequest, Action<IdentityOptions> setupAction = null)
		    where TKey : IEquatable<TKey>
		    where TUser : IdentityUser<TKey>
		    where TRole : IdentityRole<TKey>
	    {
		    MigrateToLatest<string>(connectionString);

			return identityBuilder.AddSqlStores<SqliteConnectionFactory, TKey, TUser, TRole>(connectionString, scope);
	    }

	    private static void MigrateToLatest<TKey>(string connectionString) where TKey : IEquatable<TKey>
	    {
		    var container = new ServiceCollection()
			    .AddSingleton(new ZeroMigrationContext { Database = SupportedDatabases.Sqlite })
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
