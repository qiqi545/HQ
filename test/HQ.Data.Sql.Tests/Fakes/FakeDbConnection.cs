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

using System.Data;
using System.Linq;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Queries;

namespace HQ.Data.Sql.Tests.Fakes
{
    public class FakeDbConnection : IDbConnection
    {
        private readonly ISqlDialect _dialect;

        private FakeDbCommand _command;

        public void Dispose() { }

        public IDbTransaction BeginTransaction()
        {
            return new FakeDbTransaction();
        }

        public FakeDbConnection(ISqlDialect dialect)
        {
            _dialect = dialect;
        }

        public IDbTransaction BeginTransaction(IsolationLevel il) => new FakeDbTransaction();
        public void ChangeDatabase(string databaseName) { }
        public void Close() { }
        public IDbCommand CreateCommand() => _command = new FakeDbCommand();
        public void Open() { }

        public string ConnectionString { get; set; }
        public int ConnectionTimeout => 0;
        public string Database => null;
        public ConnectionState State => ConnectionState.Closed;

        public Query GetLastQuery()
        {
            var parameters = _command.Parameters.Cast<FakeDbParameter>().ToDictionary(x => $"{_dialect.Parameter}{x.ParameterName}", v => v.Value);

            return new Query(_command.CommandText, parameters);
        }
    }
}
