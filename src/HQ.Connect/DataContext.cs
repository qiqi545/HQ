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
using System.Data;

namespace HQ.Connect
{
    public class DataContext : IDisposable
    {
        private static readonly object Sync = new object();

        private readonly IConnectionFactory _connectionFactory;
        private readonly Action<IDbConnection, IServiceProvider> _onConnection;
        private readonly IServiceProvider _serviceProvider;

        private volatile IDbConnection _connection;

        public DataContext(IConnectionFactory connectionFactory, IServiceProvider serviceProvider,
            Action<IDbConnection, IServiceProvider> onConnection = null)
        {
            _connectionFactory = connectionFactory;
            _serviceProvider = serviceProvider;
            _onConnection = onConnection;
        }

        public IDbConnection Connection => GetConnection();

        public void Dispose()
        {
            _connection?.Dispose();
            _connection = null;
        }

        private IDbConnection GetConnection()
        {
            PrimeConnection();
            return _connection;
        }

        protected void PrimeConnection()
        {
            if (_connection != null) return;
            lock (Sync)
            {
                if (_connection != null) return;
                var connection = _connectionFactory.CreateConnection();
                _onConnection?.Invoke(connection, _serviceProvider);
                connection.Open();
                _connection = connection;
            }
        }
    }
}
