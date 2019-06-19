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
using ImpromptuInterface;

namespace HQ.Data.SessionManagement
{
    public class WrapDbConnection : DbConnection
    {
        private readonly Action<IDbCommand, Type, IServiceProvider> _onCommand;
        private readonly IServiceProvider _serviceProvider;
        private readonly Type _type;
        private readonly IRetainLastInsertedId _maybeRetains;

        public WrapDbConnection(DbConnection inner, IServiceProvider serviceProvider,
            Action<IDbCommand, Type, IServiceProvider> onCommand, Type type)
        {
            Inner = inner;
            _serviceProvider = serviceProvider;
            _onCommand = onCommand;
            _type = type;

            try
            {
                _maybeRetains = Inner.ActLike(typeof(IRetainLastInsertedId));
                _maybeRetains.GetLastInsertedId();
            }
            catch
            {
                _maybeRetains = null;
            }
        }

        public DbConnection Inner { get; }

        public object LastInsertedId => _maybeRetains is IRetainLastInsertedId retainer ? retainer.GetLastInsertedId() : null;

        public override string ConnectionString
        {
            get => Inner.ConnectionString;
            set => Inner.ConnectionString = value;
        }

        public override string Database => Inner.Database;
        public override ConnectionState State => Inner.State;
        public override string DataSource => Inner.DataSource;
        public override string ServerVersion => Inner.ServerVersion;
        
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return Inner.BeginTransaction(isolationLevel);
        }

        public override void Close()
        {
            Inner.Close();
        }

        public override void Open()
        {
            Inner.Open();
        }

        protected override DbCommand CreateDbCommand()
        {
            var command = Inner.CreateCommand();
            _onCommand?.Invoke(command, _type, _serviceProvider);
            return new WrapDbCommand(command);
        }

        public override void ChangeDatabase(string databaseName)
        {
            Inner.ChangeDatabase(databaseName);
        }
    }
}

