using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace HQ.Integration.DocumentDb.Sql
{
	public class BulkExecutor : IBulkExecutor
	{
		public BulkExecutor(DocumentClient client, DocumentCollection documentCollection)
		{
			throw new NotImplementedException();
		}

		public Task<object> BulkImportAsync(IEnumerable<object> batch, bool upsert, bool b, int? optionsMaxConcurrencyPerPartitionKeyRange,
			int? optionsMaxInMemorySortingBatchSize, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<object> BulkUpdateAsync(IEnumerable<UpdateItem> patch, int? optionsMaxConcurrencyPerPartitionKeyRange,
			int? optionsMaxInMemorySortingBatchSize, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task InitializeAsync()
		{
			throw new NotImplementedException();
		}
	}
}