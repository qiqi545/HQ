using System;
using System.Data;
using System.Data.DocumentDb;
using HQ.Common.Models;
using HQ.Connect;
using HQ.Connect.DocumentDb;
using HQ.Lingo.Descriptor;
using HQ.Lingo.Dialects;
using HQ.Lingo.DocumentDb;
using HQ.Rosetta.Queryable;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Cohort.Stores.Sql.DocumentDb
{
	public static class Add
    {
	    internal const string CollectionId = "AspNetIdentity";

	    public static IdentityBuilder AddCosmosDbIdentityStore<TUser, TRole>(this IdentityBuilder identityBuilder, string connectionString, ConnectionScope scope = ConnectionScope.ByRequest, Action<IdentityOptions> setupAction = null)
			where TUser : IdentityUser<string>
		    where TRole : IdentityRole<string>
	    {
		    return AddCosmosDbIdentityStore<string, TUser, TRole>(identityBuilder, connectionString, scope, setupAction);
	    }

		internal static IdentityBuilder AddCosmosDbIdentityStore<TKey, TUser, TRole>(this IdentityBuilder identityBuilder, string connectionString, ConnectionScope scope = ConnectionScope.ByRequest, Action<IdentityOptions> setupAction = null)
		    where TKey : IEquatable<TKey>
		    where TUser : IdentityUser<TKey>
		    where TRole : IdentityRole<TKey>
	    {
		    return InstallStores<TKey, TUser, TRole>(identityBuilder, connectionString, scope, setupAction);
	    }

		private static IdentityBuilder InstallStores<TKey, TUser, TRole>(IdentityBuilder identityBuilder, string connectionString, ConnectionScope scope, Action<IdentityOptions> setupAction)
			where TKey : IEquatable<TKey>
		    where TUser : IdentityUser<TKey>
		    where TRole : IdentityRole<TKey>
	    {
		    var typeRegistry = new TypeRegistry();
			var dialect = new DocumentDbDialect();

			MigrateToLatest<TKey>(connectionString);
            
	        Cohort.Stores.Sql.Add.AddSqlStores<DocumentDbConnectionFactory, TKey, TUser, TRole>(identityBuilder,
			    connectionString, scope, setupAction, OnCommand<TKey>(typeRegistry), OnConnection);

		    identityBuilder.Services.AddSingleton<ISqlDialect>(dialect);
		    identityBuilder.Services.AddSingleton<IQueryableProvider<TUser>, DocumentDbQueryableProvider<TUser>>();
		    identityBuilder.Services.AddSingleton<IQueryableProvider<TRole>, DocumentDbQueryableProvider<TRole>>();

		    return identityBuilder;

	    }

		private static void OnConnection(IDbConnection c)
	    {
		    if (c is DocumentDbConnection connection)
		    {
				
		    }
		}

	    private static Action<IDbCommand, Type> OnCommand<TKey>(ITypeRegistry typeRegistry) where TKey : IEquatable<TKey>
	    {
		    return (c, t) =>
		    {
			    if (c is DocumentDbCommand command)
			    {
				    var descriptor = SimpleDataDescriptor.Create(t);
				    typeRegistry.RegisterIfNotRegistered(t);

					command.Id = descriptor.Id?.Property?.Name;
				    command.Type = t;
				    command.Collection = CollectionId;
			    }
		    };
	    }

	    private static void MigrateToLatest<TKey>(string connectionString) where TKey : IEquatable<TKey>
	    {
		    var runner = new MigrationRunner(connectionString);
		    runner.MigrateUp();
	    }
	}
}
