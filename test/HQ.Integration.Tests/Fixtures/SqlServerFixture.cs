using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Dapper;
using HQ.Data.SessionManagement;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Integration.Tests.Fixtures
{
	public abstract class SqlServerFixture : IServiceFixture, ISupportIsolation
    {
        public string ConnectionString => _stack.Peek().ConnectionString;

        public IServiceProvider ServiceProvider { get; set; }

        public virtual void ConfigureServices(IServiceCollection services) { }

        protected SqlServerFixture()
        {
	        _stack.Push(CreateInstance());
        }

        public void Dispose()
        {
	        DisposeInstance(_stack.Pop());
        }

        private static SqlServerInstance CreateInstance()
        {
			var instance = new SqlServerInstance();
			
	        var database = $"{Guid.NewGuid().ToString("N").ToUpperInvariant()}";
	        var builder = new SqlConnectionStringBuilder("Data Source=(localdb)\\MSSQLLocalDB;Integrated Security=true;") {InitialCatalog = "master"};
	        using (var connection = new SqlConnection(builder.ConnectionString))
	        {
		        connection.Open();
		        connection.Execute($"CREATE DATABASE [{database}] ON (NAME = N'{database}', FILENAME = '{Path.Combine(EnsureOutputDir(), database)}.mdf')");
				connection.Execute($"ALTER DATABASE [{database}] SET READ_COMMITTED_SNAPSHOT ON;");
				connection.Execute($"ALTER DATABASE [{database}] SET ALLOW_SNAPSHOT_ISOLATION ON;");
			}

	        builder.InitialCatalog = database;

	        instance.Database = database;
			instance.ConnectionString = builder.ConnectionString;
			instance.FileName = $"{Path.Combine(EnsureOutputDir(), database)}.mdf";

			return instance;
        }

		private static void DisposeInstance(SqlServerInstance instance)
        {
	        var builder = new SqlConnectionStringBuilder(instance.ConnectionString) {InitialCatalog = "master"};
	        using (var db = new SqlConnection(builder.ConnectionString))
	        {
		        db.Open();
		        try
		        {
			        db.Execute($"ALTER DATABASE [{instance.Database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
			        db.Execute($"DROP DATABASE [{instance.Database}]");
		        }
		        catch (SqlException)
		        {
		        }
	        }

	        File.Delete(Path.Combine(EnsureOutputDir(), $"{instance.Database}.mdf"));
        }

		private static string EnsureOutputDir()
		{
			var outputDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (outputDir == null)
				throw new InvalidOperationException();
			Directory.CreateDirectory(outputDir);
			return outputDir;
		}

		private readonly Stack<SqlServerInstance> _stack = new Stack<SqlServerInstance>();

		public void StartIsolation()
        {
	        if (!(ServiceProvider.GetService(typeof(IDataConnection)) is IDataConnection connection))
		        return;
	        if (connection.Transaction == null)
		        connection.Transaction = connection.Current.BeginTransaction(IsolationLevel.Snapshot);
		}

        public void EndIsolation()
        {
			if (!(ServiceProvider.GetService(typeof(IDataConnection)) is IDataConnection connection))
				return;
			connection.Transaction?.Rollback();
			connection.Transaction = null;
		}
    }
}
