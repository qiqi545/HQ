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
using HQ.Extensions.DependencyInjection;
using HQ.Extensions.DependencyInjection.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Data.SessionManagement
{
    public static class ServiceCollectionExtensions
    {
        private static DependencyContainer _container;

        public static IServiceCollection AddDatabaseConnection<TScope, TConnectionFactory>(
            this IServiceCollection services, string connectionString, ConnectionScope scope,
            Action<IDbConnection, IServiceProvider> onConnection = null,
            Action<IDbCommand, Type, IServiceProvider> onCommand = null)
            where TConnectionFactory : class, IConnectionFactory, new()
        {
            services.AddTransient(r => _container.Resolve<IDataConnection<TScope>>());

            var serviceProvider = services.BuildServiceProvider();
            _container = _container ?? new DependencyContainer(serviceProvider);
            _container.AddAspNetCore();
            _container.Register(r => serviceProvider);

            var slot = $"{typeof(TScope).FullName}";

            AddDatabaseConnection<TConnectionFactory>(services, connectionString, scope, slot, onConnection, onCommand);

            switch (scope)
            {
                case ConnectionScope.AlwaysNew:
                    _container.Register<IDataConnection<TScope>>(r =>
                        new DataConnection<TScope>(r.Resolve<DataContext>(slot), r.MustResolve<IServiceProvider>(),
                            onCommand));
                    break;
                case ConnectionScope.ByRequest:
                    _container.Register<IDataConnection<TScope>>(
                        r => new DataConnection<TScope>(r.Resolve<DataContext>(slot), r.MustResolve<IServiceProvider>(),
                            onCommand), Lifetime.Request);
                    break;
                case ConnectionScope.ByThread:
                    _container.Register<IDataConnection<TScope>>(
                        r => new DataConnection<TScope>(r.Resolve<DataContext>(slot), r.MustResolve<IServiceProvider>(),
                            onCommand), Lifetime.Thread);
                    break;
                case ConnectionScope.KeepAlive:
                    _container.Register<IDataConnection<TScope>>(
                        r => new DataConnection<TScope>(r.Resolve<DataContext>(slot), r.MustResolve<IServiceProvider>(),
                            onCommand), Lifetime.Permanent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }

            return services;
        }

		private static void AddDatabaseConnection<TConnectionFactory>(this IServiceCollection services,
			string connectionString,
			ConnectionScope scope,
			string slot,
			Action<IDbConnection, IServiceProvider> onConnection = null,
			Action<IDbCommand, Type, IServiceProvider> onCommand = null) where TConnectionFactory : class, IConnectionFactory, new()
        {
            var factory = new TConnectionFactory {ConnectionString = connectionString};

            services.AddSingleton(factory);

			var serviceProvider = services.BuildServiceProvider();
            _container = _container ?? new DependencyContainer(serviceProvider);
            _container.AddAspNetCore();
            _container.Register(slot, r => factory, Lifetime.Permanent);
            _container.Register(slot, r => serviceProvider);

            switch (scope)
            {
                case ConnectionScope.AlwaysNew:
                    _container.Register(slot,
                        r => new DataContext(r.Resolve<TConnectionFactory>(slot), r.Resolve<IServiceProvider>(), onConnection));
                    _container.Register<IDataConnection>(slot,
                        r => new DataConnection(r.Resolve<DataContext>(slot), r.Resolve<IServiceProvider>(),
                            onCommand));
                    break;
                case ConnectionScope.ByRequest:
                    _container.Register(slot,
                        r => new DataContext(r.Resolve<TConnectionFactory>(slot), r.Resolve<IServiceProvider>(), onConnection),
                        Lifetime.Request);
                    _container.Register<IDataConnection>(slot,
                        r => new DataConnection(r.Resolve<DataContext>(slot), r.Resolve<IServiceProvider>(), onCommand),
                        Lifetime.Request);
                    break;
                case ConnectionScope.ByThread:
                    _container.Register(slot,
                        r => new DataContext(r.Resolve<TConnectionFactory>(slot), r.Resolve<IServiceProvider>(), onConnection),
                        Lifetime.Thread);
                    _container.Register<IDataConnection>(slot,
                        r => new DataConnection(r.Resolve<DataContext>(slot), r.Resolve<IServiceProvider>(), onCommand),
                        Lifetime.Thread);
                    break;
                case ConnectionScope.KeepAlive:
                    _container.Register(slot,
                        r => new DataContext(r.Resolve<TConnectionFactory>(slot), r.Resolve<IServiceProvider>(), onConnection),
                        Lifetime.Permanent);
                    _container.Register<IDataConnection>(slot,
                        r => new DataConnection(r.Resolve<DataContext>(slot), r.Resolve<IServiceProvider>(), onCommand),
                        Lifetime.Permanent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }
        }
    }
}

