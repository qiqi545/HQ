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
using System.Data.DocumentDb;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkUpdate;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using HQ.Common.Models;
using HQ.Lingo.Batching;
using HQ.Lingo.Descriptor;
using HQ.Rosetta;

namespace HQ.Lingo.DocumentDb
{
    public class DocumentDbBatchDataOperation : IDataBatchOperation<DocumentDbBatchOptions>
    {
        private readonly IServerTimestampService _timestamps;
        private readonly IOptions<DocumentDbOptions> _options;
        private readonly IOptions<DocumentDbBatchOptions> _batchOptions;
        
        public DocumentDbBatchDataOperation(IServerTimestampService timestamps, IOptions<DocumentDbOptions> options, IOptions<DocumentDbBatchOptions> batchOptions)
        {
            _timestamps = timestamps;
            _options = options;
            _batchOptions = batchOptions;
        }

        public async Task<(DocumentDbBatchOptions, object)> BeforeAsync(IDbConnection connection, IDataDescriptor descriptor, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var client = connection.GetClient();

            client = new DocumentClient(client.ServiceEndpoint, client.AuthKey, Defaults.JsonSettings, client.ConnectionPolicy,
                client.ConsistencyLevel);

            client.ConnectionPolicy.ConnectionMode = ConnectionMode.Direct;
            client.ConnectionPolicy.ConnectionProtocol = Protocol.Tcp;
            client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 30;
            client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 9;

            var databaseId = connection.GetDatabaseId();

            var documentCollection = client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(databaseId))
                .Where(c => c.Id == _options.Value.CollectionId)
                .AsEnumerable().FirstOrDefault();

            IBulkExecutor bulkExecutor = new BulkExecutor(client, documentCollection);
            await bulkExecutor.InitializeAsync();

            client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 0;
            client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 0;

            return (_batchOptions.Value, bulkExecutor);
        }

        public async Task ExecuteAsync<TData>(IDbConnection connection, IDataDescriptor descriptor, DocumentDbBatchOptions options,
            object userState, BatchSaveStrategy saveStrategy, IEnumerable<TData> objects, long startingAt = 0, int? count = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            object ToDocument(TData x)
            {
                var @object = new ExpandoObject();
                var document = (IDictionary<string, object>) @object;
                document.Add("DocumentType", typeof(TData).Name);
                foreach (var property in descriptor.Inserted)
                    document.Add(property.ColumnName, property.Property.Get(x));
                return @object;
            }

            // ReSharper disable once PossibleMultipleEnumeration
            count = count ?? objects.Count();

            var client = connection.GetClient();
            var bulkExecutor = (IBulkExecutor) userState;
            var batchSize = count.GetValueOrDefault();
            
            switch (saveStrategy)
            {
                case BatchSaveStrategy.Insert:
                case BatchSaveStrategy.Upsert:
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    var data = objects.Skip((int) startingAt).Take(batchSize);

                    if (descriptor.Id != null && descriptor.Id.IsIdentity)
                    {
                        var nextValues = await client.GetNextValuesForSequenceAsync(typeof(TData),
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
                        data = data.Select(x =>
                        {
                            descriptor.Timestamp?.Property?.Set(x, _timestamps.GetCurrentTime());
                            return x;
                        });
                    }

                    var upsert = saveStrategy == BatchSaveStrategy.Upsert;

                    var batch = data.Select(ToDocument);

                    var response = await bulkExecutor.BulkImportAsync(
                        batch, upsert, false,
                        options.MaxConcurrencyPerPartitionKeyRange,
                        options.MaxInMemorySortingBatchSize, cancellationToken);

                    break;
                }
                case BatchSaveStrategy.Update:
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    var patch = objects.Select(x =>
                    {
                        var id = descriptor.Id.ColumnName;

                        var operations = descriptor.Updated.Select(p =>
                        {
                            var type = typeof(SetUpdateOperation<>).MakeGenericType(p.Property.Type);
                            var ctor = type.GetConstructor(new [] { typeof(string), p.Property.Type });
                            if (ctor == null)
                                return null;

                            var operation = (UpdateOperation) ctor.Invoke(new object[] {p.Property.Name, p.Property.Type});
                            return operation;
                        });

                        return new UpdateItem(id, id, operations.Where(o => o != null));
                    });

                    var response = await bulkExecutor.BulkUpdateAsync(patch,
                        options.MaxConcurrencyPerPartitionKeyRange,
                        options.MaxInMemorySortingBatchSize, cancellationToken);

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(saveStrategy), saveStrategy, null);
            }
           
        }

        public Task AfterAsync(IDbConnection connection, IDataDescriptor descriptor, DocumentDbBatchOptions options,
            object userState, BatchSaveStrategy saveStrategy, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Task.CompletedTask;
        }
    }
}
