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
using HQ.Harmony;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Connect
{
    public static class ServiceCollectionExtensions
    {
        private static readonly IContainer Container = new HarmonyContainer();

        public static IServiceCollection AddDatabaseConnection<T>(this IServiceCollection services,
            string connectionString, ConnectionScope scope) where T : class, IConnectionFactory, new()
        {
            return AddDatabaseConnection<T>(services, connectionString, scope, "Default");
        }

        public static IServiceCollection AddDatabaseConnection<T>(this IServiceCollection services,
            string connectionString, ConnectionScope scope, string slot) where T : class, IConnectionFactory, new()
        {
            Container.Register(slot, r => new T {ConnectionString = connectionString}, Lifetime.Permanent);

            switch (scope)
            {
                case ConnectionScope.AlwaysNew:
                    Container.Register(slot, r => new DataContext(r.Resolve<T>(slot)));
                    Container.Register<IDataConnection>(slot, r => new DataConnection(r.Resolve<DataContext>(slot)));
                    break;
                case ConnectionScope.ByRequest:
                    Container.Register(slot, r => new DataContext(r.Resolve<T>(slot)), Lifetime.Request);
                    Container.Register<IDataConnection>(slot, r => new DataConnection(r.Resolve<DataContext>(slot)),
                        Lifetime.Request);
                    break;
                case ConnectionScope.ByThread:
                    Container.Register(slot, r => new DataContext(r.Resolve<T>(slot)), Lifetime.Thread);
                    Container.Register<IDataConnection>(slot, r => new DataConnection(r.Resolve<DataContext>(slot)),
                        Lifetime.Thread);
                    break;
                case ConnectionScope.KeepAlive:
                    Container.Register(slot, r => new DataContext(r.Resolve<T>(slot)), Lifetime.Permanent);
                    Container.Register<IDataConnection>(slot, r => new DataConnection(r.Resolve<DataContext>(slot)),
                        Lifetime.Permanent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }

            services.AddTransient(r => Container.Resolve<DataContext>(slot));
            services.AddTransient(r => Container.Resolve<IDataConnection>(slot));
            return services;
        }

        public static IServiceCollection AddDatabaseConnection<TScope, TConnectionFactory>(
            this IServiceCollection services, string connectionString, ConnectionScope scope)
            where TConnectionFactory : class, IConnectionFactory, new()
        {
            var slot = $"{typeof(TScope).FullName}";

            AddDatabaseConnection<TConnectionFactory>(services, connectionString, scope, slot);

            switch (scope)
            {
                case ConnectionScope.AlwaysNew:
                    Container.Register<IDataConnection<TScope>>(r =>
                        new DataConnection<TScope>(r.Resolve<DataContext>(slot)));
                    break;
                case ConnectionScope.ByRequest:
                    Container.Register<IDataConnection<TScope>>(
                        r => new DataConnection<TScope>(r.Resolve<DataContext>(slot)), Lifetime.Request);
                    break;
                case ConnectionScope.ByThread:
                    Container.Register<IDataConnection<TScope>>(
                        r => new DataConnection<TScope>(r.Resolve<DataContext>(slot)), Lifetime.Thread);
                    break;
                case ConnectionScope.KeepAlive:
                    Container.Register<IDataConnection<TScope>>(
                        r => new DataConnection<TScope>(r.Resolve<DataContext>(slot)), Lifetime.Permanent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }

            services.AddTransient(r => Container.Resolve<IDataConnection<TScope>>());

            return services;
        }

        public static IServiceCollection AddDatabaseConnection<T>(this IServiceCollection services,
            string connectionString, Func<IConnectionFactory, DataContext> scope)
            where T : class, IConnectionFactory, new()
        {
            return AddDatabaseConnection<T>(services, connectionString, scope, "Default");
        }

        public static IServiceCollection AddDatabaseConnection<T>(IServiceCollection services, string connectionString,
            Func<IConnectionFactory, DataContext> scope, string slot)
            where T : class, IConnectionFactory, new()
        {
            Container.Register(slot, r => new T {ConnectionString = connectionString}, Lifetime.Permanent);
            Container.Register(slot, r => scope(r.Resolve<IConnectionFactory>()));
            services.AddTransient(r => Container.Resolve<DataContext>(slot));
            return services;
        }
    }
}
