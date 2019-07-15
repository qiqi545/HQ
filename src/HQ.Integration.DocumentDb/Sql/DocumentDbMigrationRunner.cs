using System;
using System.Net;
using System.Threading.Tasks;
using HQ.Integration.DocumentDb.DbProvider;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace HQ.Integration.DocumentDb.Sql
{
    public class DocumentDbMigrationRunner
    {
        private readonly DocumentDbOptions _options;

        public DocumentClient Client { get; }

        public DocumentDbMigrationRunner(DocumentDbOptions options)
        {
            _options = options;
            Client = new DocumentClient(new Uri(options.Endpoint), options.AuthKey, Defaults.JsonSettings);
        }

        public async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await Client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_options.DatabaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    await Client.CreateDatabaseAsync(new Database { Id = _options.DatabaseId });
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
