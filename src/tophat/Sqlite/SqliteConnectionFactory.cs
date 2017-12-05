using System.Data;
using Microsoft.Data.Sqlite;

namespace tophat.sqlite
{
	public class SqliteConnectionFactory : ConnectionFactory
	{
		public override IDbConnection CreateConnection()
		{
			return new SqliteConnection(ConnectionString);
		}
	}
}
