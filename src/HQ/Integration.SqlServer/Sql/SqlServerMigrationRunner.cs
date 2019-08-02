using System.Data.SqlClient;
using System.IO;
using FluentMigrator.Runner;
using MigrationRunner = HQ.Data.Sql.Migration.MigrationRunner;

namespace HQ.Integration.SqlServer.Sql
{
    public class SqlServerMigrationRunner : MigrationRunner
    {
        public SqlServerMigrationRunner(string connectionString) : base(connectionString) { }

        public override void CreateDatabaseIfNotExists()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString) { };
            if (File.Exists(builder.InitialCatalog))
                return;
            var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            connection.Close();
        }

        public override void ConfigureMigrator(IMigrationRunnerBuilder builder)
        {
            builder.AddSqlServer();
        }
    }
}
