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
using System.Reflection;

namespace HQ.Data.SessionManagement
{
    public class DataConnection : IDataConnection
    {
        private readonly DataContext _current;
        private readonly Action<IDbCommand, Type, IServiceProvider> _onCommand;
        private readonly IServiceProvider _serviceProvider;
        private Type _type;

        public DataConnection(DataContext current, IServiceProvider serviceProvider,
            Action<IDbCommand, Type, IServiceProvider> onCommand)
        {
            _current = current;
            _serviceProvider = serviceProvider;
            _onCommand = onCommand;
        }

        public void SetTypeInfo(Type type)
        {
            _type = type;
        }

        public bool TryGetLastInsertedId<TKey>(out TKey key)
        {
            if (!(Current is WrapDbConnection wrapped))
            {
                key = default;
                return false;
            }
            key = (TKey) Convert.ChangeType(wrapped.LastInsertedId, typeof(TKey));
            return true;
        }

        public IDbConnection Current
        {
            get
            {
                if (_onCommand != null && _current.Connection is DbConnection connection)
                    return new WrapDbConnection(connection, _serviceProvider, _onCommand, _type);

                return _current.Connection;
            }
        }
    }
}

