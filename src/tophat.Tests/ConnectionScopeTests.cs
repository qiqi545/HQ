using System;
using System.Data;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using tophat.sqlite;
using Xunit;

namespace tophat.Tests
{
    public class ConnectionScopeTests : DatabaseFixture
    {
        [Fact]
        public void Always_new()
        {
            var cs = CreateConnectionString();
	        var services = new ServiceCollection();
	        services.AddDatabaseConnection<SqliteConnectionFactory>(cs, ConnectionScope.AlwaysNew);

	        var provider = services.BuildServiceProvider();
	        var connection1 = provider.GetService<DataContext>();
			var connection2 = provider.GetService<DataContext>();
			Assert.NotEqual(connection1, connection2);
        }

	    [Fact]
	    public void Keep_alive()
	    {
		    var cs = CreateConnectionString();
		    var services = new ServiceCollection();
		    services.AddDatabaseConnection<SqliteConnectionFactory>(cs, ConnectionScope.KeepAlive);

		    var provider = services.BuildServiceProvider();
		    var connection1 = provider.GetService<DataContext>();
		    var connection2 = provider.GetService<DataContext>();
		    Assert.Equal(connection1, connection2);
	    }

	    [Fact]
	    public void Keep_alive_multisession()
	    {
		    var cs = CreateConnectionString();
		    var services = new ServiceCollection();
		    services.AddDatabaseConnection<SqliteConnectionFactory>(cs, ConnectionScope.KeepAlive, "one");
		    services.AddDatabaseConnection<SqliteConnectionFactory>(cs, ConnectionScope.KeepAlive, "two");

			var provider = services.BuildServiceProvider();
		    var connections = provider.GetServices<DataContext>().ToList();
		    var connection1 = connections[0];
		    var connection2 = connections[1];
		    Assert.NotEqual(connection1, connection2);
	    }
		

	    [Fact]
	    public void Keep_alive_data_connection()
		{
			var cs = CreateConnectionString();
			var services = new ServiceCollection();
			services.AddDatabaseConnection<SomeService, SqliteConnectionFactory>(cs, ConnectionScope.KeepAlive);
			services.AddDatabaseConnection<SomeOtherService, SqliteConnectionFactory>(cs, ConnectionScope.KeepAlive);
			services.AddSingleton<SomeService>();
			services.AddSingleton<SomeOtherService>();

			var provider = services.BuildServiceProvider();
			var one = provider.GetService<SomeService>();
			var two = provider.GetService<SomeOtherService>();

			Assert.NotEqual(one.Connection, two.Connection);
		}

	    public class SomeService
	    {
		    private readonly IDataConnection<SomeService> _db;
		    public IDbConnection Connection => _db.Current;

		    public SomeService(IDataConnection<SomeService> db)
		    {
			    _db = db;
		    }
	    }

	    public class SomeOtherService
	    {
		    private readonly IDataConnection<SomeOtherService> _db;
		    public IDbConnection Connection => _db.Current;
			public SomeOtherService(IDataConnection<SomeOtherService> db)
			{
				_db = db;
			}
		}

		[Fact]
		public void Resolves_custom_scoping()
		{
			var cs = CreateConnectionString();
			var services = new ServiceCollection();
			services.AddDatabaseConnection<SqliteConnectionFactory>(cs, Scope);

			var provider = services.BuildServiceProvider();
			var db = provider.GetService<DataContext>();
			Assert.NotNull(db);
			Assert.Equal(_foreverContext, provider.GetService<DataContext>());
		}

		private static DataContext _foreverContext;
        private static DataContext Scope(IConnectionFactory connectionFactory)
        {
            return _foreverContext ?? (_foreverContext = new DataContext(connectionFactory));
        }
    }
}
