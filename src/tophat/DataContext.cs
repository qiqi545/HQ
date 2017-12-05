using System;
using System.Data;

namespace tophat
{
    public class DataContext : IDisposable
    {
        private static readonly object Sync = new object();
        private volatile IDbConnection _connection;
        private readonly IConnectionFactory _connectionFactory;
        public DataContext(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public IDbConnection Connection => GetConnection();

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
                connection.Open();
                _connection = connection;
            }
        }
        public void Dispose()
        {
            _connection?.Dispose();
            _connection = null;
        }
    }
}