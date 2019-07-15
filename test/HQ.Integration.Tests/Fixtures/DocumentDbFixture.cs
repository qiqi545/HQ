using System;
using HQ.Integration.DocumentDb.SessionManagement;
using HQ.Test.Sdk;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HQ.Integration.Tests.Fixtures
{
	public abstract class DocumentDbFixture : IServiceFixture
    {
        public IServiceProvider ServiceProvider { get; set; }

        public virtual void ConfigureServices(IServiceCollection services) { }

        public void Dispose()
        {
            var options = ServiceProvider.GetRequiredService<IOptions<DocumentDbOptions>>();
            var client = new DocumentClient(new Uri(options.Value.Endpoint), options.Value.AuthKey, JsonConvert.DefaultSettings());
            client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(options.Value.DatabaseId, options.Value.CollectionId))
                .GetAwaiter().GetResult();
        }
    }
}
