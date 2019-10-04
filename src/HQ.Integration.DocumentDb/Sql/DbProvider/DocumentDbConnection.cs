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
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace HQ.Integration.DocumentDb.Sql.DbProvider
{
	public sealed class DocumentDbConnection : DbConnection
	{
		private readonly DocumentDbConnectionStringBuilder _builder = new DocumentDbConnectionStringBuilder();

		private bool _isOpen;

		public DocumentDbConnection() => _builder = new DocumentDbConnectionStringBuilder();

		public DocumentDbConnection(DocumentClient client) : this() => Client = client;

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

		public long? LastSequence { get; set; }

		public object GetLastInsertedId()
		{
			return LastSequence;
		}

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