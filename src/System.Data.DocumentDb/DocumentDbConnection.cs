// Copyright (c) HQ.IO. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace System.Data.DocumentDb
{
    public sealed class DocumentDbConnection : DbConnection
    {
        private readonly DocumentDbConnectionStringBuilder _builder = new DocumentDbConnectionStringBuilder();

        private bool _isOpen;

        public DocumentDbConnection()
        {
            _builder = new DocumentDbConnectionStringBuilder();
        }

        public DocumentDbConnection(string connectionString)
        {
            ConnectionString = connectionString;
            Client = _builder.Build();
        }

        public DocumentDbConnection(string serviceUri, string accountKey, string database)
        {
            _builder.AccountEndpoint = new Uri(serviceUri);
            _builder.AccountKey = accountKey;
            _builder.Database = database;

            Client = _builder.Build();
        }

        public override string Database => _builder.Database;
        public override string DataSource => _builder.AccountEndpoint.ToString();
        public override string ServerVersion => typeof(DocumentClient).Assembly.GetName().Version.ToString();
        public override ConnectionState State => _isOpen ? ConnectionState.Open : ConnectionState.Closed;

        public override string ConnectionString
        {
            get => _builder.ConnectionString;
            set => _builder.ConnectionString = value;
        }

        public DocumentClient Client { get; }

        public override void Open()
        {
            _isOpen = true;
        }

        public override Task OpenAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => Open(), cancellationToken);
        }

        public override void Close()
        {
            _isOpen = false;
            Client?.Dispose();
        }

        public override void ChangeDatabase(string databaseName)
        {
            Guard.AgainstNullArgument(nameof(databaseName), databaseName);
            _builder.Database = databaseName;
        }

        protected override DbCommand CreateDbCommand()
        {
            return new DocumentDbCommand(this);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }
    }
}
