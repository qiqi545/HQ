using System.Data.Common;
using System.Data.SqlClient;

namespace tophat
{
    public class SqlServerConnectionFactory : ConnectionFactory
    {
        public override DbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}