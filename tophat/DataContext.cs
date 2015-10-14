using System;
using System.Data.Common;

namespace tophat
{
    public class DataContext : IDisposable
    {
        private static readonly object Sync = new object();
        private volatile DbConnection _connection;
        private readonly IConnectionFactory _connectionFactory;
        public DataContext(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public bool IsActive => _connection != null;
        public DbConnection Connection => GetConnection();
        private DbConnection GetConnection()
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
            if (_connection == null)
            {
                return;
            }
            _connection.Dispose();
            _connection = null;
        }
    }
}