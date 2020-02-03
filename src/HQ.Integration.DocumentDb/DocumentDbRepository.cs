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
using System.Threading;
using System.Threading.Tasks;
using HQ.Data.Contracts.DataAnnotations;
using HQ.Extensions.Logging;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TypeKitchen;
using ConnectionMode = Microsoft.Azure.Cosmos.ConnectionMode;
using Database = Microsoft.Azure.Documents.Database;
using PartitionKey = Microsoft.Azure.Documents.PartitionKey;
using RequestOptions = Microsoft.Azure.Documents.Client.RequestOptions;

namespace HQ.Integration.DocumentDb
{
	public class DocumentDbRepository<T> : IDocumentDbRepository<T> where T : IDocument
	{
		// ReSharper disable once StaticMemberInGenericType
		internal static readonly RequestOptions None = new RequestOptions();
		private readonly DocumentClient _client;
		private readonly ISafeLogger<DocumentDbRepository<T>> _logger;
		private readonly IOptionsMonitor<DocumentDbOptions> _options;
		private readonly ITypeReadAccessor _reads;

		private readonly string _slot;
		private readonly ITypeWriteAccessor _writes;
		
		// 3.X
		private readonly CosmosClient _client2;
		private Microsoft.Azure.Cosmos.Database _database;
		private Container _container;
		
		public DocumentDbRepository(string slot, IOptionsMonitor<DocumentDbOptions> options,
			ISafeLogger<DocumentDbRepository<T>> logger)
		{
			_reads = ReadAccessor.Create(typeof(T));
			_writes = WriteAccessor.Create(typeof(T));
			_slot = slot;
			_options = options;
			_logger = logger;

			var defaultSettings = new JsonSerializerSettings();
			var documentDbOptions = options.Get(_slot);
			_client = new DocumentClient(EndpointUri, documentDbOptions.AccountKey, defaultSettings);


			_client2 = new CosmosClient(EndpointUri.ToString(), documentDbOptions.AccountKey,
				new CosmosClientOptions
				{
					AllowBulkExecution = true,
					ConnectionMode = ConnectionMode.Gateway,
					MaxRetryAttemptsOnRateLimitedRequests = 100
				});

			CreateDatabaseIfNotExistsAsync().Wait();
			CreateCollectionIfNotExistsAsync().Wait();
		}

		private Uri CollectionUri => UriFactory.CreateDocumentCollectionUri(_options.Get(_slot).DatabaseId, _options.Get(_slot).CollectionId);
		private Uri DatabaseUri => UriFactory.CreateDatabaseUri(_options.Get(_slot).DatabaseId);
		private Uri EndpointUri => _options.Get(_slot).AccountEndpoint;

		public async Task<Document> CreateAsync(T item, CancellationToken cancellationToken = default)
		{
			await BeforeSaveAsync(item, cancellationToken);
			var document = await _client.CreateDocumentAsync(CollectionUri, item, cancellationToken: cancellationToken);
			return document;
		}

		public async Task<T> RetrieveAsync(string id, CancellationToken cancellationToken = default)
		{
			try
			{
				var uri = DocumentUri(id);
				var options = GetRequestOptions(id, _options.CurrentValue);
				Document document = await _client.ReadDocumentAsync(uri, options, cancellationToken);
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

		public Task<long> CountAsync(Expression<Func<T, bool>> predicate = null,
			CancellationToken cancellationToken = default)
		{
			var queryable = CreateDocumentQuery();
			var query = predicate != null ? queryable.Where(predicate).Count() : queryable.Count();
			return Task.FromResult((long) query);
		}

		public async Task<IEnumerable<T>> RetrieveAsync(Func<IQueryable<T>, IQueryable<T>> projection,
			CancellationToken cancellationToken = default)
		{
			var queryable = projection(CreateDocumentQuery());

			var result = await GetResultsAsync(queryable.AsDocumentQuery(), cancellationToken);

			return result;
		}

		public async Task<IEnumerable<T>> RetrieveAsync(Expression<Func<T, bool>> predicate = null,
			CancellationToken cancellationToken = default)
		{
			var queryable = CreateDocumentQuery();

			var query = predicate != null ? queryable.Where(predicate).AsDocumentQuery() : queryable.AsDocumentQuery();

			return await GetResultsAsync(query, cancellationToken);
		}

		public async Task<T> RetrieveSingleAsync(Expression<Func<T, bool>> predicate = null,
			CancellationToken cancellationToken = default)
		{
			var results = await RetrieveAsync(predicate, cancellationToken);

			return results.Single();
		}

		public async Task<T> RetrieveSingleOrDefaultAsync(Expression<Func<T, bool>> predicate = null,
			CancellationToken cancellationToken = default)
		{
			var results = await RetrieveAsync(predicate, cancellationToken);

			return results.SingleOrDefault();
		}

		public async Task<T> RetrieveFirstAsync(Expression<Func<T, bool>> predicate = null,
			CancellationToken cancellationToken = default)
		{
			var results = await RetrieveAsync(predicate, cancellationToken);

			return results.First();
		}

		public async Task<T> RetrieveFirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null,
			CancellationToken cancellationToken = default)
		{
			var results = await RetrieveAsync(predicate, cancellationToken);

			return results.FirstOrDefault();
		}

		public async Task<Document> UpdateAsync(string id, T item, CancellationToken cancellationToken = default)
		{
			return await _client.ReplaceDocumentAsync(DocumentUri(id), item, GetRequestOptions(id, _options.CurrentValue), cancellationToken);
		}

		public async Task<Document> UpsertAsync(T item, CancellationToken cancellationToken = default)
		{
			var response = await _client.UpsertDocumentAsync(CollectionUri, item, GetRequestOptions(item.Id, _options.CurrentValue), cancellationToken: cancellationToken);
			return response;
		}

		public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
		{
			_database ??= await CreateDatabaseIfNotExistsAsync2(cancellationToken);
			_container ??= await CreateContainerIfNotExistsAsync(cancellationToken);

			var response = await _container.DeleteItemAsync<T>(id, new Microsoft.Azure.Cosmos.PartitionKey(id), new ItemRequestOptions(), cancellationToken);
			return response.StatusCode == HttpStatusCode.NoContent;
		}

		public async Task<bool> DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
		{
			_database ??= await CreateDatabaseIfNotExistsAsync2(cancellationToken);
			_container ??= await CreateContainerIfNotExistsAsync(cancellationToken);

			var deleted = 0;
			var total = ids.Count();
			foreach(var id in ids)
			{
				try
				{
					var options = new ItemRequestOptions();
					await _container.DeleteItemAsync<T>(id, new Microsoft.Azure.Cosmos.PartitionKey(id), options, cancellationToken);
					deleted++;
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
			}

			return deleted == total;
		}

		internal static RequestOptions GetRequestOptions(string id, DocumentDbOptions options = null)
		{
			var o = new RequestOptions();

			if (options != null)
				o.OfferThroughput = options.OfferThroughput;

			if(!string.IsNullOrWhiteSpace(id))
				o.PartitionKey = new PartitionKey(id);

			return o;
		}

		private async Task BeforeSaveAsync(T item, CancellationToken cancellationToken)
		{
			await ValidateUniqueFields(item, cancellationToken);

			await TryUpdateAutoIncrementingValues(item);
		}

		private async Task ValidateUniqueFields(T item, CancellationToken cancellationToken)
		{
			IQueryable<T> queryable = null;
			foreach (var member in AccessorMembers.Create(typeof(T)))
			{
				if (!member.HasAttribute<UniqueAttribute>())
				{
					continue;
				}

				queryable ??= CreateDocumentQuery();
				queryable = queryable.Where(ComputedPredicate<T>.AsExpression(member.Name, ExpressionOperator.Equal,
					_reads[item, member.Name]));
			}

			if (queryable == null)
			{
				return;
			}

			var query = queryable.AsDocumentQuery();
			var results = await GetResultsAsync(query, cancellationToken);
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

		private async Task<Microsoft.Azure.Cosmos.Database> CreateDatabaseIfNotExistsAsync2(CancellationToken cancellationToken = default)
		{
			var databaseName = _options.CurrentValue.DatabaseId;
			var response = await _client2.CreateDatabaseIfNotExistsAsync(databaseName, cancellationToken: cancellationToken);
			return response.Database;
		}

		private async Task<Container> CreateContainerIfNotExistsAsync(CancellationToken cancellationToken = default)
		{
			var containerName = _options.CurrentValue.CollectionId;
			var response = await _database.CreateContainerIfNotExistsAsync(containerName, 
				_options.CurrentValue.PartitionKeyPath,
				_options.CurrentValue.OfferThroughput, cancellationToken: cancellationToken);

			return response.Container;
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
			var databaseId = _options.Get(_slot).DatabaseId;
			var collectionId = _options.Get(_slot).CollectionId;
			var uri = UriFactory.CreateDocumentUri(databaseId, collectionId, id);
			return uri;
		}

		private IQueryable<T> CreateDocumentQuery()
		{
			IQueryable<T> queryable = _client.CreateDocumentQuery<T>(CollectionUri,
				new FeedOptions {MaxItemCount = -1, EnableCrossPartitionQuery = true});

			if (_options.Get(_slot).SharedCollection)
			{
				queryable = queryable.Where(x => x.DocumentType == DocumentTypeFactory<T>.Type);
			}

			return queryable;
		}

		private static async Task<List<T>> GetResultsAsync(IDocumentQuery<T> query, CancellationToken cancellationToken)
		{
			try
			{
				var results = new List<T>();
				while (query.HasMoreResults)
				{
					results.AddRange(await query.ExecuteNextAsync<T>(cancellationToken));
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