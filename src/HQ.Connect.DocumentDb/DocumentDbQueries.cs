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
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HQ.Cadence;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace HQ.Connect.DocumentDb
{
    internal abstract class DocumentDbQueries
    {
        private static readonly FeedOptions EmptyOptions = new FeedOptions();
        private static readonly FeedOptions SingleItemOptions = new FeedOptions {MaxItemCount = 1};
        private readonly DocumentClient _client;
        private readonly Uri _collectionUri;

        private readonly IMetricsHost<DocumentClient> _metrics;

        protected DocumentDbQueries(DocumentClient client, Uri collectionUri, IMetricsHost<DocumentClient> metrics)
        {
            _client = client;
            _collectionUri = collectionUri;
            _metrics = metrics;
        }

        public async Task<T> SingleOrDefaultAsync<T>(Expression<Func<T, bool>> predicate)
        {
            return (await FindByPredicateAsync(predicate, SingleItemOptions)).SingleOrDefault();
        }

        public T SingleOrDefault<T>(Expression<Func<T, bool>> predicate)
        {
            return FindByPredicate(predicate, SingleItemOptions).SingleOrDefault();
        }

        public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate)
        {
            return (await FindByPredicateAsync(predicate, SingleItemOptions)).FirstOrDefault();
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate)
        {
            return FindByPredicate(predicate, SingleItemOptions).FirstOrDefault();
        }

        protected FeedResponse<T> FindByPredicate<T>(Expression<Func<T, bool>> predicate, FeedOptions options = default)
        {
            var queryable = _client.CreateDocumentQuery<T>(_collectionUri, options ?? EmptyOptions);
            var query = queryable.Where(predicate);
            var response = query.AsDocumentQuery().ExecuteNextAsync<T>().ConfigureAwait(false).GetAwaiter().GetResult();
            _metrics.Gauge(nameof(FeedResponse<T>.RequestCharge), () => response.RequestCharge);
            return response;
        }

        protected async Task<FeedResponse<T>> FindByPredicateAsync<T>(Expression<Func<T, bool>> predicate,
            FeedOptions options = default)
        {
            var queryable = _client.CreateDocumentQuery<T>(_collectionUri, options ?? EmptyOptions);
            var query = queryable.Where(predicate);
            var response = await query.AsDocumentQuery().ExecuteNextAsync<T>();
            _metrics.Gauge(nameof(FeedResponse<T>.RequestCharge), () => response.RequestCharge);
            return response;
        }
    }
}
