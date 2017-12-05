using System.Data;

namespace tophat
{
    public abstract class ConnectionFactory : IConnectionFactory
    {
        public abstract IDbConnection CreateConnection();
        public string ConnectionString { get; set; }
    }
}