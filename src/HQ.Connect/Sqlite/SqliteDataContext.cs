namespace tophat.sqlite
{
	public class SqliteDataContext : DataContext
	{
		public SqliteDataContext(string connectionString) : base(new SqliteConnectionFactory { ConnectionString = connectionString })
		{

		}
	}
}