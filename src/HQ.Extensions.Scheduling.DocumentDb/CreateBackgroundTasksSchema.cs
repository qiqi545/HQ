using System.Net;
using System.Threading.Tasks;
using HQ.Data.SessionManagement.DocumentDb;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace HQ.Extensions.Scheduling.DocumentDb
{
    public class CreateBackgroundTasksSchema
    {
        private readonly DocumentClient _client;
        private readonly DocumentDbOptions _options;

        public CreateBackgroundTasksSchema(DocumentClient client, DocumentDbOptions options)
        {
            _client = client;
            _options = options;
        }

        public async Task Up()
        {
            await CreateCollectionIfNotExistsAsync();
        }

        private async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await _client.ReadDocumentCollectionAsync(
                    UriFactory.CreateDocumentCollectionUri(_options.DatabaseId, _options.CollectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(_options.DatabaseId),
                        new DocumentCollection { Id = _options.CollectionId },
                        new RequestOptions { OfferThroughput = _options.OfferThroughput });
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
