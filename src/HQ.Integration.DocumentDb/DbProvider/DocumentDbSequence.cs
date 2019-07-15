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

namespace HQ.Integration.DocumentDb.DbProvider
{
	/// <summary>
	///     Provides virtual sequences through a software-only pseudo-atomic counter.
	///     If you don't need fine-grained control over the sequence value, use DocumentDB's DocumentId function,
	///     i.e. `SELECT DocumentId(r) FROM r`, or map the document's `_rid` field.
	/// </summary>
	public static class DocumentDbSequence
	{
		public static async Task<(long, long)> GetNextValuesForSequenceAsync(this DocumentClient client,
			Type documentType, string databaseId, string collectionId, int count)
		{
			var uri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

			var document = new ExpandoObject();
			var sequence = (IDictionary<string, object>) document;
			sequence.Add(Constants.DocumentTypeField, Constants.SequenceDocumentType);
			sequence.Add(Constants.SequenceTypeField, documentType.Name);

			var sql =
				$@"SELECT r.id, r.Current FROM {Constants.SequenceDocumentType} r WHERE r.{Constants.DocumentTypeField} = @{Constants.DocumentTypeField} AND r.{Constants.SequenceTypeField} = @{Constants.SequenceTypeField}";
			var query = new SqlQuerySpec(sql);
			query.Parameters.Add(new SqlParameter($"@{Constants.DocumentTypeField}",
				sequence[Constants.DocumentTypeField]));
			query.Parameters.Add(new SqlParameter($"@{Constants.SequenceTypeField}",
				sequence[Constants.SequenceTypeField]));

			var feedOptions = new FeedOptions {MaxItemCount = 1};
			var result = client.CreateDocumentQuery<Sequence>(uri, query, feedOptions).AsDocumentQuery();
			var nextSequence = (await result.ExecuteNextAsync<Sequence>()).SingleOrDefault();

			var startAt = nextSequence.Current + 1;
			var endAt = nextSequence.Current + count;

			sequence[Constants.IdKey] = nextSequence.id;
			sequence[nameof(Sequence.Current)] = endAt;

			var requestOptions = new RequestOptions();
			await client.UpsertDocumentAsync(uri, document, requestOptions);

			return (startAt, endAt);
		}

		public static async Task<long> GetNextValueForSequenceAsync(this DocumentClient client, Type documentType,
			string databaseId, string collectionId)
		{
			var nextValue = await GetNextValuesForSequenceAsync(client, documentType, databaseId, collectionId, 1);
			return nextValue.Item2;
		}

		public static async Task<long> SetNextValueForSequenceAsync(this DocumentClient client,
			IDictionary<string, object> document, string propertyName, Type documentType, string databaseId,
			string collectionId)
		{
			var nextValue = await client.GetNextValueForSequenceAsync(documentType, databaseId, collectionId);

			var options = new RequestOptions();
			var disableAutomaticIdGeneration = document.ContainsKey(Constants.IdKey);

			var uri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
			document[propertyName] = nextValue;
			await client.UpsertDocumentAsync(uri, document, options, disableAutomaticIdGeneration);

			return nextValue;
		}

		private struct Sequence
		{
			// ReSharper disable once InconsistentNaming
			public string id;
			public long Current;
		}
	}
}