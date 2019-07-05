using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Dapper;
using HQ.Data.SessionManagement;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Test.Sdk.Data
{
    public abstract class SqlServerFixture : IServiceFixture, ISupportIsolation
    {
        private readonly string _database;

        public string ConnectionString { get; }

        public IServiceProvider ServiceProvider { get; set; }

        public virtual void ConfigureServices(IServiceCollection services) { }

        protected SqlServerFixture()
        {
            ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Integrated Security=true;";

            var outputDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (outputDir == null)
                throw new InvalidOperationException();
            Directory.CreateDirectory(outputDir);

            var database = $"{Guid.NewGuid().ToString("N").ToUpperInvariant()}";
            var builder = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" };
            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                connection.Execute($"CREATE DATABASE [{database}] ON (NAME = N'{database}', FILENAME = '{Path.Combine(outputDir, database)}.mdf')");
                connection.Execute($"ALTER DATABASE [{database}] SET READ_COMMITTED_SNAPSHOT ON;");
                connection.Execute($"ALTER DATABASE [{database}] SET ALLOW_SNAPSHOT_ISOLATION ON;");
            }

            builder.InitialCatalog = database;
            _database = database;
            ConnectionString = builder.ConnectionString;
        }

        public void Dispose()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" };
            using (var db = new SqlConnection(builder.ConnectionString))
            {
                db.Open();
                try
                {
                    db.Execute($"ALTER DATABASE [{_database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
                    db.Execute($"DROP DATABASE [{_database}]");
                }
                catch (SqlException) { }
            }
            var outputDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (outputDir == null)
                throw new InvalidOperationException();
            Directory.CreateDirectory(outputDir);
            File.Delete(Path.Combine(outputDir, $"{_database}.mdf"));
        }

        public void StartIsolation()
        {
            if(ServiceProvider.GetService(typeof(IDataConnection)) is IDataConnection connection)
            {
                if (connection.Transaction != null)
                    connection.Transaction = connection.Current.BeginTransaction(IsolationLevel.Snapshot);
            }
        }

        public void EndIsolation()
        {
            if (ServiceProvider.GetService(typeof(IDataConnection)) is IDataConnection connection)
            {
                connection.Transaction?.Rollback();
                connection.Transaction = null;
            }
        }
    }
}
