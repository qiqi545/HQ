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
using HQ.Harmony;
using HQ.Harmony.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Connect
{
    public static class ServiceCollectionExtensions
    {
        private static HarmonyContainer _container;

        public static IServiceCollection AddDatabaseConnection<T>(this IServiceCollection services,
            string connectionString, ConnectionScope scope) where T : class, IConnectionFactory, new()
        {
            _container = _container ?? new HarmonyContainer(services.BuildServiceProvider());
            _container.AddAspNetCore();

            return AddDatabaseConnection<T>(services, connectionString, scope, "Default");
        }

        public static IServiceCollection AddDatabaseConnection<TScope, TConnectionFactory>(
            this IServiceCollection services, string connectionString, ConnectionScope scope,
            Action<IDbConnection> onConnection = null, Action<IDbCommand, Type> onCommand = null)
            where TConnectionFactory : class, IConnectionFactory, new()
        {
            services.AddTransient(r => _container.Resolve<IDataConnection<TScope>>());

            _container = _container ?? new HarmonyContainer(services.BuildServiceProvider());
            _container.AddAspNetCore();

            var slot = $"{typeof(TScope).FullName}";

            AddDatabaseConnection<TConnectionFactory>(services, connectionString, scope, slot, onConnection, onCommand);

            switch (scope)
            {
                case ConnectionScope.AlwaysNew:
                    _container.Register<IDataConnection<TScope>>(r =>
                        new DataConnection<TScope>(r.Resolve<DataContext>(slot), onCommand));
                    break;
                case ConnectionScope.ByRequest:
                    _container.Register<IDataConnection<TScope>>(
                        r => new DataConnection<TScope>(r.Resolve<DataContext>(slot), onCommand), Lifetime.Request);
                    break;
                case ConnectionScope.ByThread:
                    _container.Register<IDataConnection<TScope>>(
                        r => new DataConnection<TScope>(r.Resolve<DataContext>(slot), onCommand), Lifetime.Thread);
                    break;
                case ConnectionScope.KeepAlive:
                    _container.Register<IDataConnection<TScope>>(
                        r => new DataConnection<TScope>(r.Resolve<DataContext>(slot), onCommand), Lifetime.Permanent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }

            return services;
        }

        public static IServiceCollection AddDatabaseConnection<T>(this IServiceCollection services,
            string connectionString, ConnectionScope scope, string slot, Action<IDbConnection> onConnection = null,
            Action<IDbCommand, Type> onCommand = null) where T : class, IConnectionFactory, new()
        {
            var factory = new T {ConnectionString = connectionString};

            services.AddTransient(r => _container.Resolve<T>(slot));
            services.AddTransient(r => _container.Resolve<DataContext>(slot));
            services.AddTransient(delegate { return _container.Resolve<IDataConnection>(slot); });

            _container = _container ?? new HarmonyContainer(services.BuildServiceProvider());
            _container.AddAspNetCore();
            _container.Register(slot, r => factory, Lifetime.Permanent);

            switch (scope)
            {
                case ConnectionScope.AlwaysNew:
                    _container.Register(slot, r => new DataContext(r.Resolve<T>(slot), onConnection));
                    _container.Register<IDataConnection>(slot,
                        r => new DataConnection(r.Resolve<DataContext>(slot), onCommand));
                    break;
                case ConnectionScope.ByRequest:
                    _container.Register(slot, r => new DataContext(r.Resolve<T>(slot), onConnection), Lifetime.Request);
                    _container.Register<IDataConnection>(slot,
                        r => new DataConnection(r.Resolve<DataContext>(slot), onCommand), Lifetime.Request);
                    break;
                case ConnectionScope.ByThread:
                    _container.Register(slot, r => new DataContext(r.Resolve<T>(slot), onConnection), Lifetime.Thread);
                    _container.Register<IDataConnection>(slot,
                        r => new DataConnection(r.Resolve<DataContext>(slot), onCommand), Lifetime.Thread);
                    break;
                case ConnectionScope.KeepAlive:
                    _container.Register(slot, r => new DataContext(r.Resolve<T>(slot), onConnection),
                        Lifetime.Permanent);
                    _container.Register<IDataConnection>(slot,
                        r => new DataConnection(r.Resolve<DataContext>(slot), onCommand), Lifetime.Permanent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }

            return services;
        }

        public static IServiceCollection AddDatabaseConnection<T>(this IServiceCollection services,
            string connectionString, Func<IConnectionFactory, DataContext> scope)
            where T : class, IConnectionFactory, new()
        {
            _container = _container ?? new HarmonyContainer(services.BuildServiceProvider());
            _container.AddAspNetCore();

            return AddDatabaseConnection<T>(services, connectionString, scope, "Default");
        }

        public static IServiceCollection AddDatabaseConnection<T>(IServiceCollection services, string connectionString,
            Func<IConnectionFactory, DataContext> scope, string slot)
            where T : class, IConnectionFactory, new()
        {
            var factory = new T {ConnectionString = connectionString};

            services.AddTransient(r => _container.Resolve<T>(slot));
            services.AddTransient(r => _container.Resolve<DataContext>(slot));

            _container = _container ?? new HarmonyContainer(services.BuildServiceProvider());
            _container.AddAspNetCore();
            _container.Register(slot, r => factory, Lifetime.Permanent);
            _container.Register(slot, r => scope(r.Resolve<IConnectionFactory>()));

            return services;
        }
    }
}
