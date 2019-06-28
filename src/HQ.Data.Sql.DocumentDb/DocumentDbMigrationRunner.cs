using System.Data.DocumentDb;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace HQ.Data.Sql.DocumentDb
{
    public class DocumentDbMigrationRunner
    {
        private readonly DocumentDbConnectionStringBuilder _builder;
        private readonly DocumentClient _client;

        public DocumentDbMigrationRunner(string connectionString)
        {
            _builder = new DocumentDbConnectionStringBuilder(connectionString);
            _client = _builder.Build();
        }

        public async Task CreateDatabaseIfNotExistsAsync()
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
    }
}
