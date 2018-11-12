using System;
using System.Data.Common;
using System.Diagnostics;
using container;

namespace tophat
{
    public static partial class Database
    {
        internal static Container Container;

        static Database()
        {
            Container = new Container();
            Initialize();
        }

        public static void Install<T>(string connectionString, ConnectionScope scope = ConnectionScope.ByRequest) where T : class, IConnectionFactory, new()
        {
            ResetContexts();
            var lifetime = GetLifetime(scope);
            Container.Register(r => new DataContext(r.Resolve<IConnectionFactory>()), lifetime);
            RegisterConnectionFactory<T>(scope, connectionString);
        }

        public static void Install<T>(string connectionString, Func<IConnectionFactory, DataContext> scope) where T : class, IConnectionFactory, new()
        {
            ResetContexts();
            Container.Register(r => scope(r.Resolve<IConnectionFactory>()));
            RegisterConnectionFactory<T>(ConnectionScope.AlwaysNew, connectionString);
        }

        public static void Install(string connectionString, Func<string, DbConnection> proxy, ConnectionScope scope = ConnectionScope.ByRequest)
        {
            ResetContexts();
            var lifetime = GetLifetime(scope);
            Container.Register(r => new DataContext(r.Resolve<IConnectionFactory>()), lifetime);
            RegisterConnectionFactory(scope, connectionString, proxy);
        }

        public static void Install(string connectionString, Func<string, DbConnection> proxy, Func<IConnectionFactory, DataContext> scope)
        {
            ResetContexts();
            Container.Register(r => scope(r.Resolve<IConnectionFactory>()));
            RegisterConnectionFactory(ConnectionScope.AlwaysNew, connectionString, proxy);
        }

        private static void ResetContexts()
        {
            var old = Container;
            Container = new Container();
            old.Dispose();
        }

        private static void RegisterConnectionFactory<T>(ConnectionScope scope, string connectionString) where T : class, IConnectionFactory, new()
        {
            var lifetime = GetLifetime(scope);
            Container.Register<IConnectionFactory>(r => new T { ConnectionString = connectionString }, lifetime);
            Debug.Assert(Container.Resolve<IConnectionFactory>() != null, "Database registration did not resolve. Check your ConnectionScope, it must not be able to return null in the current context.");
        }

        private static void RegisterConnectionFactory(ConnectionScope scope, string connectionString, Func<string, DbConnection> proxy)
        {
            var lifetime = GetLifetime(scope);
            Container.Register<IConnectionFactory>(r => new ProxyConnectionFactory { Proxy = proxy, ConnectionString = connectionString }, lifetime);
            Debug.Assert(Container.Resolve<IConnectionFactory>() != null, "Database registration did not resolve. Check your ConnectionScope, it must not be able to return null in the current context.");
        }

        private static Lifetime GetLifetime(ConnectionScope scope)
        {
            switch (scope)
            {
                case ConnectionScope.ByRequest:
                    return Lifetime.Request;
                case ConnectionScope.ByThread:
                    return Lifetime.Thread;
                case ConnectionScope.KeepAlive:
                    return Lifetime.Permanent;
                case ConnectionScope.AlwaysNew:
                    return Lifetime.AlwaysNew;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope));
            }
        }

        public static void Purge()
        {
            SafeDispose();
            Initialize();
        }
    }
}