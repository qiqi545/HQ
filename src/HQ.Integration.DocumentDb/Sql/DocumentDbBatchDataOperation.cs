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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Data.Contracts;
using HQ.Data.Sql.Batching;
using HQ.Data.Sql.Descriptor;
using HQ.Integration.DocumentDb.DbProvider;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HQ.Integration.DocumentDb.Sql
{
	// See: https://docs.microsoft.com/en-us/azure/cosmos-db/performance-tips
	// See: https://github.com/Azure/azure-cosmos-dotnet-v2/blob/master/samples/documentdb-benchmark/Program.cs
	// See: https://docs.microsoft.com/en-us/azure/cosmos-db/bulk-executor-dot-net (deprecated)

	public class DocumentDbBatchDataOperation : IDataBatchOperation<DocumentDbBatchOptions>
    {
        private readonly IOptions<DocumentDbBatchOptions> _batchOptions;
        private readonly IOptions<DocumentDbOptions> _options;
        private readonly IServerTimestampService _timestamps;

        public DocumentDbBatchDataOperation(IServerTimestampService timestamps, IOptions<DocumentDbOptions> options,
            IOptions<DocumentDbBatchOptions> batchOptions)
        {
            _timestamps = timestamps;
            _options = options;
            _batchOptions = batchOptions;
        }

		private readonly ConcurrentDictionary<Uri, DocumentClient> _bulkClients = new ConcurrentDictionary<Uri, DocumentClient>();

		public Task<(DocumentDbBatchOptions, object)> BeforeAsync(IDbConnection connection,
			IDataDescriptor descriptor, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			var client = RequisitionBulkCopyClient(connection);

			var databaseId = connection.GetDatabaseId();

			var documentCollection = client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(databaseId))
				.Where(c => c.Id == _options.Value.CollectionId)
				.AsEnumerable().FirstOrDefault();

			(DocumentDbBatchOptions, object) result = (_batchOptions.Value, documentCollection);

			return Task.FromResult(result);
		}

		private DocumentClient RequisitionBulkCopyClient(IDbConnection connection)
		{
			var perConnection = connection.GetClient();
			var client = _bulkClients.GetOrAdd(perConnection.ServiceEndpoint,
				endpoint => CreateBulkCopyClient(endpoint, perConnection));
			return client;
		}

		private static DocumentClient CreateBulkCopyClient(Uri endpoint, IDocumentClient template)
		{
			var client = new DocumentClient(endpoint, template.AuthKey, Defaults.JsonSettings,
				ConnectionPolicy, template.ConsistencyLevel);

			// See: https://docs.microsoft.com/en-us/azure/cosmos-db/performance-tips
			client.OpenAsync().GetAwaiter().GetResult();

			return client;
		}

		private static readonly ConnectionPolicy ConnectionPolicy = new ConnectionPolicy
		{
			ConnectionMode = ConnectionMode.Direct,
			ConnectionProtocol = Protocol.Tcp,
			RequestTimeout = new TimeSpan(1, 0, 0),
			MaxConnectionLimit = 1000,
			RetryOptions = new RetryOptions
			{
				MaxRetryAttemptsOnThrottledRequests = 10,
				MaxRetryWaitTimeInSeconds = 60
			}
		};

		public async Task ExecuteAsync<TData>(IDbConnection connection, IDataDescriptor descriptor,
            DocumentDbBatchOptions options,
            object userState, BatchSaveStrategy saveStrategy, IEnumerable<TData> objects, long startingAt = 0,
            int? count = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
	        var client = RequisitionBulkCopyClient(connection);
	        var collection = (DocumentCollection) userState;
	        var partitionKey = collection.PartitionKey.Paths[0].Replace("/", "");
			
	        var throughput = 400;
	        var offer = client.CreateOfferQuery().Where(o => o.ResourceLink == collection.SelfLink)
		        .AsEnumerable().FirstOrDefault();
	        if (offer != null)
		        throughput = ((OfferV2) offer).Content.OfferThroughput;

			// ReSharper disable once PossibleMultipleEnumeration
			count = count ?? objects.Count();

            var batchSize = count.GetValueOrDefault();

            switch (saveStrategy)
            {
	            case BatchSaveStrategy.Insert:
	            {
		            // ReSharper disable once PossibleMultipleEnumeration
		            var data = objects.Skip((int) startingAt).Take(batchSize);

		            if (descriptor.Id != null && descriptor.Id.IsIdentity)
		            {
			            var nextValues = await client.GetNextValuesForSequenceAsync(typeof(TData).Name,
				            connection.GetDatabaseId(), _options.Value.CollectionId, batchSize);

			            // ReSharper disable once PossibleMultipleEnumeration
			            data = data.Select((x, i) =>
			            {
				            descriptor.Id.Property.Set(x, nextValues.Item1 + i);
				            return x;
			            });
		            }

		            if (descriptor.Timestamp != null)
		            {
			            var timestamp = _timestamps.GetCurrentTime();
			            data = data.Select(x =>
			            {
				            descriptor.Timestamp?.Property?.Set(x, timestamp);
				            return x;
			            });
		            }

		            var batch = data.Select(x =>
		            {
						var @object = new ExpandoObject();
			            var document = (IDictionary<string, object>) @object;
			            document.Add("DocumentType", typeof(TData).Name);
						foreach (var property in descriptor.Inserted)
			            {
				            document.Add(property.ColumnName, property.Property.Get(x));
			            }
						document["id"] = Guid.NewGuid().ToString();
						document[partitionKey] = Guid.NewGuid().ToString();
			            return @object;
					});

					// set TaskCount = 10 for each 10k RUs, minimum 1, maximum 250
					var taskCount = Math.Min(Math.Max(throughput / 1000, 1), 250);
		            var tasks = new List<Task>();
		            var pageSize = batchSize / taskCount;
		            var databaseId = connection.GetDatabaseId();
		            for (var i = 0; i < taskCount; i++)
		            {
			            // ReSharper disable once PossibleMultipleEnumeration
			            var page = batch.Skip(i * pageSize).Take(pageSize);
			            tasks.Add(InsertDocumentAsync(client, databaseId, collection, page, pageSize));
		            }
		            await Task.WhenAll(tasks);
		            break;
	            }

	            case BatchSaveStrategy.Upsert:
                {
					throw new NotImplementedException();
				}

                case BatchSaveStrategy.Update:
                {
                    throw new NotImplementedException();
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(saveStrategy), saveStrategy, null);
            }
        }

		private static async Task InsertDocumentAsync(IDocumentClient client, string databaseId, Resource collection, IEnumerable<ExpandoObject> page, int pageSize)
		{
			var options = new RequestOptions() { };

			// FIXME: collection.SelfLink?
			var documentCollectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collection.Id);

			foreach (var document in page)
			{
				try
				{
					await client.CreateDocumentAsync(documentCollectionUri, document, options);
				}
				catch (Exception e)
				{
					if (e is DocumentClientException documentException)
					{
						if (documentException.StatusCode != HttpStatusCode.Forbidden)
						{
							var json = JsonConvert.SerializeObject(document);
							Trace.TraceError("Failed to write {0}. Exception was {1}", json, e);
						}
					}
				}
			}
		}

		public Task AfterAsync(IDbConnection connection, IDataDescriptor descriptor, DocumentDbBatchOptions options,
            object userState, BatchSaveStrategy saveStrategy, IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            return Task.CompletedTask;
        }
    }
}
