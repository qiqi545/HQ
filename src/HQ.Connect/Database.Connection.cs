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

namespace HQ.Connect
{
    public static partial class Database
    {
        private static Lazy<IConnectionFactory> _factory;

        public static IConnectionFactory ConnectionFactory => _factory.Value;

        public static DbConnection Current => ConnectionFactory == null
            ? null
            : Adapter(ConnectionFactory.GetUnitOfWorkScopedConnection());

        public static Func<DbConnection, DbConnection> Adapter { get; set; }

        public static DbTransaction Transaction { get; private set; }

        private static Lazy<IConnectionFactory> CreateFactoryMethod()
        {
            return new Lazy<IConnectionFactory>(() =>
            {
                var connectionFactory = Container.Resolve<IConnectionFactory>();
                return connectionFactory;
            });
        }

        private static void Initialize()
        {
            _factory = CreateFactoryMethod();
            Adapter = @default => @default;
        }

        public static void InTransaction(Action<DbConnection> db, Action<Exception> onError = null,
            IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            try
            {
                using (var transaction = Current.BeginTransaction(isolation))
                {
                    Transaction = transaction;
                    db(Current);
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
            finally
            {
                Transaction = null;
            }
        }

        public static void InTransaction(Action closure, Action<Exception> onError = null,
            IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            try
            {
                using (var transaction = Current.BeginTransaction(isolation))
                {
                    Transaction = transaction;
                    closure();
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
            finally
            {
                Transaction = null;
            }
        }

        public static void SafeDispose()
        {
            if (ConnectionFactory == null || !ConnectionFactory.IsActive)
                return;

            Current.Dispose();
            ConnectionFactory.Reset();
        }
    }
}
