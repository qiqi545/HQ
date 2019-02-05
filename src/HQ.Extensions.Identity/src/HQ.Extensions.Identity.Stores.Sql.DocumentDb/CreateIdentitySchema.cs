#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System.Net;
using System.Threading.Tasks;
using HQ.Data.Sql.DocumentDb;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace HQ.Extensions.Identity.Stores.Sql.DocumentDb
{
    public class CreateIdentitySchema
    {
        private readonly DocumentClient _client;
        private readonly string _databaseId;
        private readonly DocumentDbOptions _options;

        public CreateIdentitySchema(DocumentClient client, string databaseId, DocumentDbOptions options)
        {
            _client = client;
            _databaseId = databaseId;
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
                    UriFactory.CreateDocumentCollectionUri(_databaseId, _options.CollectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                    await _client.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(_databaseId),
                        new DocumentCollection {Id = _options.CollectionId},
                        new RequestOptions {OfferThroughput = _options.OfferThroughput});
                else
                    throw;
            }
        }
    }
}
