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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Data.Contracts;
using HQ.Data.Sql.Batching;
using HQ.Data.Sql.Descriptor;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;

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
        private readonly ConcurrentDictionary<Uri, DocumentClient> _bulkClients = new ConcurrentDictionary<Uri, DocumentClient>();

        public DocumentDbBatchDataOperation(IServerTimestampService timestamps, IOptions<DocumentDbOptions> options,
            IOptions<DocumentDbBatchOptions> batchOptions)
        {
            _timestamps = timestamps;
            _options = options;
            _batchOptions = batchOptions;
        }

		public Task<(DocumentDbBatchOptions, object)> BeforeAsync(IDbConnection connection,
			IDataDescriptor descriptor, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			var client = DocumentDbBulkExecutor.RequisitionBulkCopyClient(connection.GetClient());
			var databaseId = connection.GetDatabaseId();

			var documentCollection = client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(databaseId))
				.Where(c => c.Id == _options.Value.CollectionId)
				.AsEnumerable().FirstOrDefault();

			(DocumentDbBatchOptions, object) result = (_batchOptions.Value, documentCollection);
			return Task.FromResult(result);
		}

		public async Task ExecuteAsync<TData>(IDbConnection connection, IDataDescriptor descriptor,
            DocumentDbBatchOptions options,
            object userState, BatchSaveStrategy saveStrategy, IEnumerable<TData> objects, long startingAt = 0,
            int? count = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
	        var client = DocumentDbBulkExecutor.RequisitionBulkCopyClient(connection.GetClient());
	        var collection = (DocumentCollection) userState;
	        var databaseId = connection.GetDatabaseId();

			await DocumentDbBulkExecutor.ExecuteAsync(descriptor, _timestamps, saveStrategy, objects, startingAt, count, client, databaseId, collection);
        }
		 
		public Task AfterAsync(IDbConnection connection, IDataDescriptor descriptor, DocumentDbBatchOptions options,
            object userState, BatchSaveStrategy saveStrategy, IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            return Task.CompletedTask;
        }
    }
}
