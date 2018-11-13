// Copyright (c) HQ.IO. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;

namespace System.Data.DocumentDb
{
    public sealed class DocumentDbProviderFactory : DbProviderFactory
    {
        private readonly string _authKey;
        private readonly string _databaseName;
        private readonly string _serviceUri;

        public DocumentDbProviderFactory(string serviceUri, string authKey, string databaseName)
        {
            _serviceUri = serviceUri;
            _authKey = authKey;
            _databaseName = databaseName;
        }

        public override DbCommand CreateCommand()
        {
            return new DocumentDbCommand();
        }

        public override DbConnection CreateConnection()
        {
            return new DocumentDbConnection(_serviceUri, _authKey, _databaseName);
        }

        public override DbParameter CreateParameter()
        {
            return new DocumentDbParameter();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return new DocumentDbDataAdapter();
        }
    }
}
