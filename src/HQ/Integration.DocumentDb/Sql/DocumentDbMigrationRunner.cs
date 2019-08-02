using System.Collections.ObjectModel;
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
            Client = new DocumentClient(options.AccountEndpoint, options.AccountKey, Defaults.JsonSettings);
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

        public async Task CreateCollectionIfNotExistsAsync()
        {
	        try
	        {
		        await Client.ReadDocumentCollectionAsync(
			        UriFactory.CreateDocumentCollectionUri(_options.DatabaseId, _options.CollectionId));
	        }
	        catch (DocumentClientException e)
	        {
		        if (e.StatusCode == HttpStatusCode.NotFound)
		        {
			        await Client.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(_options.DatabaseId),
				        new DocumentCollection
				        {
					        Id = _options.CollectionId, PartitionKey = 
								new PartitionKeyDefinition
								{
									Paths = new Collection<string>(_options.PartitionKeyPaths),
									Version = PartitionKeyDefinitionVersion.V2
								}
				        },
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
