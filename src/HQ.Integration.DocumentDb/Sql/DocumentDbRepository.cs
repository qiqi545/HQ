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
using HQ.Extensions.CodeGeneration.Scripting;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TypeKitchen;

namespace HQ.Integration.DocumentDb.Sql
{
    public class DocumentDbRepository<T> : IDocumentDbRepository<T> where T : IDocument
    {
        private readonly ITypeReadAccessor _reads;
        private readonly ITypeWriteAccessor _writes;

        private readonly string _slot;
		private readonly DocumentClient _client;
        private readonly IOptionsMonitor<DocumentDbOptions> _options;
        
        public DocumentDbRepository(string slot, IOptionsMonitor<DocumentDbOptions> options)
        {
            _reads = ReadAccessor.Create(typeof(T));
            _writes = WriteAccessor.Create(typeof(T));
            _slot = slot;
            _options = options;

            var defaultSettings = new JsonSerializerSettings();
            _client = new DocumentClient(EndpointUri, options.Get(_slot).AccountKey, defaultSettings);

            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();
        }

        private Uri CollectionUri => UriFactory.CreateDocumentCollectionUri(_options.Get(_slot).DatabaseId, _options.Get(_slot).CollectionId);
        private Uri DatabaseUri => UriFactory.CreateDatabaseUri(_options.Get(_slot).DatabaseId);
        private Uri EndpointUri => _options.Get(_slot).AccountEndpoint;

        public async Task<Document> CreateAsync(T item)
        {
            await BeforeSaveAsync(item);

            var document = await _client.CreateDocumentAsync(CollectionUri, item);
            
            return document;
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

        public async Task<T> RetrieveSingleAsync(Expression<Func<T, bool>> predicate = null)
        {
            var results = await RetrieveAsync(predicate);

            return results.Single();
        }

        public async Task<T> RetrieveSingleOrDefaultAsync(Expression<Func<T, bool>> predicate = null)
        {
            var results = await RetrieveAsync(predicate);

            return results.SingleOrDefault();
        }

        public async Task<T> RetrieveFirstAsync(Expression<Func<T, bool>> predicate = null)
        {
            var results = await RetrieveAsync(predicate);

            return results.First();
        }

        public async Task<T> RetrieveFirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null)
        {
            var results = await RetrieveAsync(predicate);

            return results.FirstOrDefault();
        }

        public async Task<Document> UpdateAsync(string id, T item)
        {
            return await _client.ReplaceDocumentAsync(DocumentUri(id), item);
        }

        public async Task DeleteAsync(string id)
        {
            await _client.DeleteDocumentAsync(DocumentUri(id));
        }

        private async Task BeforeSaveAsync(T item)
        {
            await ValidateUniqueFields(item);

            await TryUpdateAutoIncrementingValues(item);
        }

        private async Task ValidateUniqueFields(T item)
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
                    _reads[item, member.Name]));
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
        
        private async Task TryUpdateAutoIncrementingValues(T item)
        {
            foreach (var member in AccessorMembers.Create(typeof(T)))
            {
                if (!member.HasAttribute<AutoIncrementAttribute>())
                    continue;

                if (_reads[item, member.Name] == default)
                    continue;

                var value = await _client.GetNextValueForSequenceAsync($"AutoIncrement_{typeof(T).Name}_{member.Name}",
	                _options.Get(_slot).DatabaseId, _options.Get(_slot).CollectionId);

                var typed = Convert.ChangeType(value, member.Type);
                _writes[item, member.Name] = typed;
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
                    await _client.CreateDatabaseAsync(new Database {Id = _options.Get(_slot).DatabaseId});
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
                        new DocumentCollection {Id = _options.Get(_slot).CollectionId},
                        new RequestOptions {OfferThroughput = _options.Get(_slot).OfferThroughput});
                }
                else
                {
                    throw;
                }
            }
        }

        private Uri DocumentUri(string id)
        {
            return UriFactory.CreateDocumentUri(_options.Get(_slot).DatabaseId, _options.Get(_slot).CollectionId, id);
        }

        private IQueryable<T> CreateDocumentQuery()
        {
            IQueryable<T> queryable = _client.CreateDocumentQuery<T>(CollectionUri,
                new FeedOptions {MaxItemCount = -1});

            if (_options.Get(_slot).SharedCollection)
            {
                queryable = queryable.Where(x => x.DocumentType == DocumentTypeFactory<T>.Type);
            }

            return queryable;
        }

        private static async Task<List<T>> GetResultsAsync(IDocumentQuery<T> query)
        {
            try
            {
                var results = new List<T>();
                while (query.HasMoreResults)
                {
                    results.AddRange(await query.ExecuteNextAsync<T>());
                }
                return results;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
