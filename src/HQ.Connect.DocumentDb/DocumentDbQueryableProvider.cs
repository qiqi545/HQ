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
using HQ.Lingo.DocumentDb;
using HQ.Lingo.Queries;
using HQ.Rosetta.Queryable;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;

namespace HQ.Connect.DocumentDb
{
    public class DocumentDbQueryableProvider<T> : IQueryableProvider<T>
    {
        private readonly DocumentDbConnectionFactory _factory;
        private readonly IMetricsHost<DocumentClient> _metrics;
        private readonly IOptions<DocumentDbOptions> _options;

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
            ? throw new NotSupportedException(
                "Direct IQueryable access is not supported for DocumentDb. Use Queries instead.")
            : (IQueryable<T>) null;

        public IQueryable<T> UnsafeQueryable
        {
            get
            {
                var connection = (DocumentDbConnection) _factory.CreateConnection();
                var collectionUri =
                    UriFactory.CreateDocumentCollectionUri(connection.Database, _options.Value.CollectionId);
                return connection.Client.CreateDocumentQuery<T>(collectionUri);
            }
        }

        public ISafeQueryable<T> SafeQueryable
        {
            get
            {
                var connection = (DocumentDbConnection) _factory.CreateConnection();
                var collectionUri =
                    UriFactory.CreateDocumentCollectionUri(connection.Database, _options.Value.CollectionId);
                return new DocumentDbSafeQueryable<T>(connection.Client, collectionUri, _metrics);
            }
        }

        public IEnumerable<T> SafeAll
        {
            get
            {
                var connection = (DocumentDbConnection) _factory.CreateConnection();
                var command = connection.CreateCommand();

                var descriptor = SqlBuilder.GetDescriptor<T>();
                var select = SqlBuilder.Select<T>(descriptor);
                command.CommandText = select.Sql;

                var query = command.ToQuerySpec();

                if (command is DocumentDbCommand docDbCommand)
                {
                    docDbCommand.Id = descriptor.Id?.Property?.Name;
                    docDbCommand.Type = typeof(T);
                    docDbCommand.DocumentType = descriptor.Table;
                    docDbCommand.MaybeTypeDiscriminate(query);
                }

                // IMPORTANT: This is a direct call, there is no ADO.NET pass-through here. This means that
                // the JSON.NET serializer configuration is the only step in converting the typed document into
                // and object (to be serialized again back to JSON in the output).

                var collectionUri =
                    UriFactory.CreateDocumentCollectionUri(connection.Database, _options.Value.CollectionId);
                return connection.Client.CreateDocumentQuery<T>(collectionUri, query);
            }
        }
    }
}
