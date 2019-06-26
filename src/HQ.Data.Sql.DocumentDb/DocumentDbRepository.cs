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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using HQ.Data.Contracts.Attributes;
using HQ.Data.SessionManagement.DocumentDb;
using HQ.Extensions.CodeGeneration.Scripting;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TypeKitchen;

namespace HQ.Data.Sql.DocumentDb
{
    public class DocumentDbRepository<T> : IDocumentDbRepository<T> where T : IDocument
    {
        private readonly ITypeReadAccessor _accessor;
        private readonly DocumentClient _client;
        private readonly IOptions<DocumentDbOptions> _options;

        public DocumentDbRepository(IOptions<DocumentDbOptions> options)
        {
            _accessor = ReadAccessor.Create(typeof(T));
            _options = options;
            _client = new DocumentClient(EndpointUri, options.Value.AuthKey, JsonConvert.DefaultSettings());

            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();
        }

        private Uri CollectionUri =>
            UriFactory.CreateDocumentCollectionUri(_options.Value.DatabaseId, _options.Value.CollectionId);

        private Uri DatabaseUri => UriFactory.CreateDatabaseUri(_options.Value.DatabaseId);
        private Uri EndpointUri => new Uri(_options.Value.Endpoint);

        public async Task<Document> CreateAsync(T item)
        {
            await BeforeSave(item);

            return await _client.CreateDocumentAsync(CollectionUri, item);
        }

        public async Task<T> RetrieveAsync(string id)
        {
            try
            {
                Document document = await _client.ReadDocumentAsync(DocumentUri(id));
                return (T) (dynamic) document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    return default;
                }

                throw;
            }
        }

        public async Task<IEnumerable<T>> RetrieveAsync(Func<IQueryable<T>, IQueryable<T>> projection)
        {
            var queryable = projection(CreateDocumentQuery());

            var result = await GetResultsAsync(queryable.AsDocumentQuery());

            return result;
        }

        public async Task<IEnumerable<T>> RetrieveAsync(Expression<Func<T, bool>> predicate = null)
        {
            var queryable = CreateDocumentQuery();

            var query = predicate != null ? queryable.Where(predicate).AsDocumentQuery() : queryable.AsDocumentQuery();

            return await GetResultsAsync(query);
        }

        public async Task<Document> UpdateAsync(string id, T item)
        {
            return await _client.ReplaceDocumentAsync(DocumentUri(id), item);
        }

        public async Task DeleteAsync(string id)
        {
            await _client.DeleteDocumentAsync(DocumentUri(id));
        }

        private async Task BeforeSave(T item)
        {
            IQueryable<T> queryable = null;
            foreach (var member in AccessorMembers.Create(typeof(T)))
            {
                if (!member.HasAttribute<UniqueAttribute>())
                {
                    continue;
                }

                queryable = queryable ?? CreateDocumentQuery();
                queryable = queryable.Where(ComputedPredicate<T>.AsExpression(member.Name, ExpressionOperator.Equal,
                    _accessor[item, member.Name]));
            }

            if (queryable == null)
            {
                return;
            }

            var query = queryable.AsDocumentQuery();
            var results = await GetResultsAsync(query);
            if (results.Count > 0)
            {
                throw new DataException("Creating document would violate unique constraints for the document type.");
            }
        }

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await _client.ReadDatabaseAsync(DatabaseUri);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDatabaseAsync(new Database {Id = _options.Value.DatabaseId});
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await _client.ReadDocumentCollectionAsync(CollectionUri);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentCollectionAsync(DatabaseUri,
                        new DocumentCollection {Id = _options.Value.CollectionId},
                        new RequestOptions {OfferThroughput = _options.Value.OfferThroughput});
                }
                else
                {
                    throw;
                }
            }
        }

        private Uri DocumentUri(string id)
        {
            return UriFactory.CreateDocumentUri(_options.Value.DatabaseId, _options.Value.CollectionId, id);
        }

        private IQueryable<T> CreateDocumentQuery()
        {
            IQueryable<T> queryable = _client.CreateDocumentQuery<T>(CollectionUri,
                new FeedOptions {MaxItemCount = -1});

            if (_options.Value.SharedCollection)
            {
                queryable = queryable.Where(x => x.DocumentType == DocumentTypeFactory<T>.Type);
            }

            return queryable;
        }

        private static async Task<List<T>> GetResultsAsync(IDocumentQuery<T> query)
        {
            var results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }
    }
}
