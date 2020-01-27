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
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Data.Contracts;
using HQ.Data.Sql.Descriptor;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace HQ.Integration.DocumentDb.Sql
{
	public class DocumentDbBulkExecutor
	{
		private static readonly ConcurrentDictionary<Uri, DocumentClient> BulkClients =
			new ConcurrentDictionary<Uri, DocumentClient>();

		private static readonly ConnectionPolicy ConnectionPolicy = new ConnectionPolicy
		{
			ConnectionMode = ConnectionMode.Direct,
			ConnectionProtocol = Protocol.Tcp,
			RequestTimeout = new TimeSpan(1, 0, 0),
			MaxConnectionLimit = 1000,
			RetryOptions = new RetryOptions
			{
				MaxRetryAttemptsOnThrottledRequests = 10, MaxRetryWaitTimeInSeconds = 60
			}
		};

		public static DocumentClient RequisitionBulkCopyClient(IDocumentClient template)
		{
			var client = BulkClients.GetOrAdd(template.ServiceEndpoint,
				endpoint => CreateBulkCopyClient(endpoint, template));
			return client;
		}

		public static DocumentClient CreateBulkCopyClient(Uri endpoint, IDocumentClient template)
		{
			var client = new DocumentClient(endpoint, template.AuthKey, Defaults.JsonSettings, ConnectionPolicy,
				template.ConsistencyLevel);

			// See: https://docs.microsoft.com/en-us/azure/cosmos-db/performance-tips
			client.OpenAsync().GetAwaiter().GetResult();

			return client;
		}

		public static async Task ExecuteAsync<TData>(IDataDescriptor descriptor, IServerTimestampService timestamps,
			BatchSaveStrategy saveStrategy, IEnumerable<TData> objects,
			long startingAt, int? count, DocumentClient client, string databaseId, Resource collection)
		{
			var offer = client.CreateOfferQuery().Where(o => o.ResourceLink == collection.SelfLink)
				.AsEnumerable().FirstOrDefault();
			var throughput = ((OfferV2) offer)?.Content.OfferThroughput ?? 1000;

			// ReSharper disable once PossibleMultipleEnumeration
			count ??= objects.Count();

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
							databaseId, collection.Id, batchSize);

						// ReSharper disable once PossibleMultipleEnumeration
						data = data.Select((x, i) =>
						{
							descriptor.Id.Property.Set(x, nextValues.Item1 + i);
							return x;
						});
					}

					if (descriptor.Timestamp != null)
					{
						var timestamp = timestamps.GetCurrentTime();
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
						foreach (var property in descriptor.Inserted)
							document.Add(property.ColumnName, property.Property.Get(x));
						if (!document.ContainsKey("DocumentType"))
							document.Add("DocumentType", typeof(TData).Name);

						document["id"] = Guid.NewGuid().ToString();
						return @object;
					});

					// set TaskCount = 10 for each 10k RUs, minimum 1, maximum 250
					var taskCount = Math.Min(Math.Max(throughput / 1000, 1), 250);
					var tasks = new List<Task>();
					var pageSize = batchSize / taskCount;
					for (var i = 0; i < taskCount; i++)
					{
						// ReSharper disable once PossibleMultipleEnumeration
						var page = batch.Skip(i * pageSize).Take(pageSize);
						tasks.Add(InsertDocumentAsync(client, databaseId, collection, page));
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

		private static async Task InsertDocumentAsync(IDocumentClient client, string databaseId, Resource collection, IEnumerable<ExpandoObject> page)
		{
			// FIXME: collection.SelfLink?
			var documentCollectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collection.Id);

			foreach (var document in page)
			{
				try
				{
					await client.CreateDocumentAsync(documentCollectionUri, document, DocumentDbRepository<IDocument>.GetRequestOptions());
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
	}
}