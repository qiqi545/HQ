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

using System.Data.Common;

namespace HQ.Integration.DocumentDb.DbProvider
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