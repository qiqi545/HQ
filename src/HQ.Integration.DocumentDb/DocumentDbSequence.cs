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
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;

namespace HQ.Integration.DocumentDb
{
	/// <summary>
	///     Provides virtual sequences through a software-only pseudo-atomic counter.
	///     If you don't need fine-grained control over the sequence value, use DocumentDB's DocumentId function,
	///     i.e. `SELECT DocumentId(r) FROM r`, or map the document's `_rid` field.
	/// </summary>
	public static class DocumentDbSequence
	{
		public const string IdKey = "id";
		public const string SequenceDocumentType = "Sequence";
		public const string SequenceTypeField = "SequenceType";
		public const string DocumentTypeField = "DocumentType";

		public static async Task<long> GetNextValueForSequenceAsync(this DocumentClient client, string sequenceTypeName,
			string databaseId, string collectionId)
		{
			var nextValue = await GetNextValuesForSequenceAsync(client, sequenceTypeName, databaseId, collectionId, 1);
			return nextValue.Item2;
		}

		public static async Task<(long, long)> GetNextValuesForSequenceAsync(this DocumentClient client,
			string sequenceTypeName, string databaseId, string collectionId, int count)
		{
			var uri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

			var document = new ExpandoObject();
			var sequence = (IDictionary<string, object>) document;
			sequence.Add(DocumentTypeField, SequenceDocumentType);
			sequence.Add(SequenceTypeField, sequenceTypeName);

			var sql = $@"SELECT r.id, r.Current FROM {SequenceDocumentType} r WHERE r.{DocumentTypeField} = @{DocumentTypeField} AND r.{SequenceTypeField} = @{SequenceTypeField}";
			var query = new SqlQuerySpec(sql);
			query.Parameters.Add(new SqlParameter($"@{DocumentTypeField}", sequence[DocumentTypeField]));
			query.Parameters.Add(new SqlParameter($"@{SequenceTypeField}", sequence[SequenceTypeField]));

			var feedOptions = new FeedOptions {MaxItemCount = 1, EnableCrossPartitionQuery = true};
			var result = client.CreateDocumentQuery<Sequence>(uri, query, feedOptions).AsDocumentQuery();

			try
			{
				var nextSequence = (await result.ExecuteNextAsync<Sequence>()).SingleOrDefault();
				var startAt = nextSequence.Current + 1;
				var endAt = nextSequence.Current + count;

				sequence[IdKey] = nextSequence.Id;
				sequence[nameof(Sequence.Current)] = endAt;

				var requestOptions = new RequestOptions();
				await client.UpsertDocumentAsync(uri, document, requestOptions);
				return (startAt, endAt);
			}
			catch (DocumentClientException e)
			{
				Console.WriteLine(e);
				throw;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}


		public struct Sequence
		{
			[JsonProperty("id")] public string Id { get; set; }

			[JsonProperty("current")] public long Current { get; set; }
		}
	}
}