using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace System.Data.DocumentDb
{
    public static class DocumentDbSequence
    {
        public static async Task<(long, long)> GetNextValuesForSequenceAsync(this DocumentClient client, Type documentType, string databaseId, string collectionId, int count)
        {
            var uri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

            var sequence = new Dictionary<string, object>
            {
                [Constants.DocumentTypeField] = Constants.SequenceDocumentType,
                [Constants.SequenceTypeField] = documentType.Name
            };

            var sql = $@"SELECT r.id, r.Current FROM {Constants.SequenceDocumentType} r WHERE r.{Constants.DocumentTypeField} = @{Constants.DocumentTypeField} AND r.{Constants.SequenceTypeField} = @{Constants.SequenceTypeField}";
            var query = new SqlQuerySpec(sql);
            query.Parameters.Add(new SqlParameter($"@{Constants.DocumentTypeField}", sequence[Constants.DocumentTypeField]));
            query.Parameters.Add(new SqlParameter($"@{Constants.SequenceTypeField}", sequence[Constants.SequenceTypeField]));

            var feedOptions = new FeedOptions { MaxItemCount = 1 };
            var result = client.CreateDocumentQuery<Sequence>(uri, query, feedOptions).AsDocumentQuery();
            var nextSequence = (await result.ExecuteNextAsync<Sequence>()).SingleOrDefault();

            var startAt = nextSequence.Current + 1;
            var endAt = nextSequence.Current + count;

            sequence[Constants.IdKey] = nextSequence.id;
            sequence[nameof(Sequence.Current)] = endAt;

            var requestOptions = new RequestOptions();
            await client.UpsertDocumentAsync(uri, sequence, requestOptions, true);

            return (startAt, endAt);
        }

        public static async Task<long> GetNextValueForSequenceAsync(this DocumentClient client, Type documentType, string databaseId, string collectionId)
        {
            var nextValue = await GetNextValuesForSequenceAsync(client, documentType, databaseId, collectionId, 1);
            return nextValue.Item2;
        }

        public static async Task<long> SetNextValueForSequenceAsync(this DocumentClient client, IDictionary<string, object> document, string propertyName, Type documentType, string databaseId, string collectionId)
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
