using System;
using System.Data;
using System.Data.Common;

namespace tophat
{
    public static partial class Database
    {
        private static Lazy<IConnectionFactory> _factory;

        private static Lazy<IConnectionFactory> CreateFactoryMethod()
        {
            return new Lazy<IConnectionFactory>(() =>
            {
                var connectionFactory = Container.Resolve<IConnectionFactory>();
                return connectionFactory;
            });
        }

        public static IConnectionFactory ConnectionFactory => _factory.Value;

        private static void Initialize()
        {
            _factory = CreateFactoryMethod();
            Adapter = @default => @default;
        }

        public static DbConnection Current => ConnectionFactory == null ? null : Adapter(ConnectionFactory.GetUnitOfWorkScopedConnection());
        public static Func<DbConnection, DbConnection> Adapter { get; set; }

        public static void InTransaction(Action<DbConnection> db, Action<Exception> onError = null, IsolationLevel isolation = IsolationLevel.ReadCommitted)
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

        public static void InTransaction(Action closure, Action<Exception> onError = null, IsolationLevel isolation = IsolationLevel.ReadCommitted)
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

        public static DbTransaction Transaction { get; private set; }

        public static void SafeDispose()
        {
            if (ConnectionFactory == null || !ConnectionFactory.IsActive)
                return;

            Current.Dispose();
            ConnectionFactory.Reset();
        }
    }
}