using System;
using System.Data.Common;

namespace tophat
{
    public class ProxyConnectionFactory : ConnectionFactory
    {
        public Func<string, DbConnection> Proxy { get; set; }

        public override DbConnection CreateConnection()
        {
            var connection = Proxy(ConnectionString);
            return connection;
        }
    }
}