using System.Data.DocumentDb;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace HQ.Cohort.Stores.Sql.DocumentDb
{
	public class MigrationRunner
	{
		private readonly DocumentClient _client;
		private readonly DocumentDbConnectionStringBuilder _builder;

		public MigrationRunner(string connectionString)
		{
			_builder = new DocumentDbConnectionStringBuilder(connectionString);
			_client = _builder.Build();
		}

		private async Task CreateDatabaseIfNotExistsAsync()
		{
			try
			{
				await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_builder.Database));
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == HttpStatusCode.NotFound)
				{
					await _client.CreateDatabaseAsync(new Database { Id = _builder.Database });
				}
				else
				{
					throw;
				}
			}
		}

		public void MigrateUp()
		{
			CreateDatabaseIfNotExistsAsync().Wait();

			new CreateIdentitySchema(_client, _builder.Database).Up().Wait();
		}
	}
}
