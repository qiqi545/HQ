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
using System.Data.DocumentDb;
using System.Linq;
using HQ.Cadence;
using HQ.Rosetta.Queryable;
using Microsoft.Azure.Documents.Client;
using HQ.Connect.DocumentDb.Configuration;
using Microsoft.Extensions.Options;
using HQ.Lingo.Queries;

namespace HQ.Connect.DocumentDb
{
    public class DocumentDbQueryableProvider<T> : IQueryableProvider<T>
    {
        private readonly IOptions<DocumentDbOptions> _options;
        private readonly DocumentDbConnectionFactory _factory;
        private readonly IMetricsHost<DocumentClient> _metrics;

        public DocumentDbQueryableProvider(DocumentDbConnectionFactory factory, IMetricsHost<DocumentClient> metrics,
            IOptions<DocumentDbOptions> options)
        {
            _options = options;
            _factory = factory;
            _metrics = metrics;
        }

        public virtual bool IsSafe => false;
        public virtual bool SupportsUnsafe => true;

        /// <summary>
        ///     CosmosDb has too many out-of-band considerations to provide a safe <see cref="IQueryable" />.
        ///     Instead, use <see cref="SafeQueryable" /> when you want predictable data access using expression predicates.
        /// </summary>
        public IQueryable<T> Queryable => IsSafe
            ? throw new NotSupportedException("Direct IQueryable access is not supported for DocumentDb. Use Queries instead.")
            : (IQueryable<T>) null;

        public IQueryable<T> UnsafeQueryable
        {
            get
            {
                var connection = (DocumentDbConnection)_factory.CreateConnection();
                var collectionUri = UriFactory.CreateDocumentCollectionUri(connection.Database, _options.Value.CollectionId);
                return connection.Client.CreateDocumentQuery<T>(collectionUri);
            }
        }

        public ISafeQueryable<T> SafeQueryable
        {
            get
            {
                var connection = (DocumentDbConnection) _factory.CreateConnection();
                var collectionUri = UriFactory.CreateDocumentCollectionUri(connection.Database, _options.Value.CollectionId);
                return new DocumentDbSafeQueryable<T>(connection.Client, collectionUri, _metrics);
            }
        }

        public IEnumerable<T> SafeAll
        {
            get
            {
                var connection = (DocumentDbConnection)_factory.CreateConnection();
                var command = connection.CreateCommand();

                var select = SqlBuilder.Select<T>();
                command.CommandText = select.Sql;

                var query = command.ToQuerySpec();

                if (command is DocumentDbCommand docDbCommand)
                {
                    docDbCommand.Type = typeof(T);
                    docDbCommand.MaybeTypeDiscriminate(query);
                }

                var collectionUri = UriFactory.CreateDocumentCollectionUri(connection.Database, _options.Value.CollectionId);
                return connection.Client.CreateDocumentQuery<T>(collectionUri, query);
            }
        }
    }
}

