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
using System.Data.Common;

namespace HQ.Data.SessionManagement
{
    public class WrapDbCommand : DbCommand
    {
        private readonly DbCommand _inner;

        public WrapDbCommand(DbCommand inner)
        {
            _inner = inner;
        }

        public override void Cancel()
        {
            _inner.Cancel();
        }

        public override int ExecuteNonQuery()
        {
            var result = _inner.ExecuteNonQuery();
            return result;
        }

        public override object ExecuteScalar()
        {
            var result = _inner.ExecuteScalar();
            return result;
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return _inner.ExecuteReader(behavior);
        }

        #region Passthrough

        public override void Prepare()
        {
            _inner.Prepare();
        }

        public override string CommandText
        {
            get => _inner.CommandText;
            set => _inner.CommandText = value;
        }

        public override int CommandTimeout
        {
            get => _inner.CommandTimeout;
            set => _inner.CommandTimeout = value;
        }

        public override CommandType CommandType
        {
            get => _inner.CommandType;
            set => _inner.CommandType = value;
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => _inner.UpdatedRowSource;
            set => _inner.UpdatedRowSource = value;
        }

        protected override DbConnection DbConnection
        {
            get => _inner.Connection;
            set => _inner.Connection = value;
        }

        protected override DbParameterCollection DbParameterCollection => _inner.Parameters;

        protected override DbTransaction DbTransaction
        {
            get => _inner.Transaction;
            set => _inner.Transaction = value;
        }

        public override bool DesignTimeVisible
        {
            get => _inner.DesignTimeVisible;
            set => _inner.DesignTimeVisible = value;
        }

        protected override DbParameter CreateDbParameter()
        {
            return _inner.CreateParameter();
        }

        #endregion
    }
}

