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

using System.Data;
using System.Linq;
using HQ.Data.SessionManagement;
using HQ.Integration.Sqlite.SessionManagement;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HQ.Data.SessionManagement.Tests
{
    public class ConnectionScopeTests : DatabaseFixture
    {
        public class SomeService
        {
            private readonly IDataConnection<SomeService> _db;

            public SomeService(IDataConnection<SomeService> db)
            {
                _db = db;
            }

            public IDbConnection Connection => _db.Current;
        }

        public class SomeOtherService
        {
            private readonly IDataConnection<SomeOtherService> _db;

            public SomeOtherService(IDataConnection<SomeOtherService> db)
            {
                _db = db;
            }

            public IDbConnection Connection => _db.Current;
        }

        private static DataContext _foreverContext;

        private static DataContext Scope(IConnectionFactory connectionFactory)
        {
            return _foreverContext ?? (_foreverContext = new DataContext(connectionFactory, null));
        }

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

        [Fact]
        public void Keep_alive_multisession()
        {
            var cs = CreateConnectionString();
            var services = new ServiceCollection();
			services.AddDatabaseConnection<SqliteConnectionFactory>(cs, ConnectionScope.KeepAlive);
            services.AddDatabaseConnection<SqliteConnectionFactory>(cs, ConnectionScope.KeepAlive);

            var provider = services.BuildServiceProvider();
            var connections = provider.GetServices<DataContext>().ToList();
            var connection1 = connections[0];
            var connection2 = connections[1];
            Assert.NotEqual(connection1, connection2);
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
    }
}
