namespace tophat
{
    public class SqlServerDataContext : DataContext
    {
        public SqlServerDataContext(string connectionString) : base(new SqlServerConnectionFactory { ConnectionString = connectionString})
        {

        }
    }
}