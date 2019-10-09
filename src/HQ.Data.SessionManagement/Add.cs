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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using HQ.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Data.SessionManagement
{
	public static class Add
	{
		private static readonly ConcurrentDictionary<string, IContainer> Containers = new ConcurrentDictionary<string, IContainer>();
		
		public static ContainerBuilder AddDatabaseConnection<TScope, TConnectionFactory>(
			this IServiceCollection services, string connectionString, ConnectionScope scope,
			IEnumerable<IResolverExtension> extensions = null,
			Action<IDbConnection, IServiceProvider> onConnection = null,
			Action<IDbCommand, Type, IServiceProvider> onCommand = null)
			where TConnectionFactory : class, IConnectionFactory, new()
		{
			var slot = $"{typeof(TScope).FullName}";
			var container = services.AddContainer(slot);

			services.AddTransient(r => container.Resolve<IDataConnection<TScope>>());
	
			var builder = AddDatabaseConnection<TConnectionFactory>(services, connectionString, scope, slot, extensions, onConnection, onCommand);

			switch (scope)
			{
				case ConnectionScope.AlwaysNew:
					container.Register<IDataConnection<TScope>>(r =>
						new DataConnection<TScope>(r.Resolve<DataContext>(slot), r.MustResolve<IServiceProvider>(),
							onCommand));
					break;
				case ConnectionScope.ByRequest:
					container.Register<IDataConnection<TScope>>(
						r => new DataConnection<TScope>(r.Resolve<DataContext>(slot), r.MustResolve<IServiceProvider>(),
							onCommand), Lifetime.Request);
					break;
				case ConnectionScope.ByThread:
					container.Register<IDataConnection<TScope>>(
						r => new DataConnection<TScope>(r.Resolve<DataContext>(slot), r.MustResolve<IServiceProvider>(),
							onCommand), Lifetime.Thread);
					break;
				case ConnectionScope.KeepAlive:
					container.Register<IDataConnection<TScope>>(
						r => new DataConnection<TScope>(r.Resolve<DataContext>(slot), r.MustResolve<IServiceProvider>(),
							onCommand), Lifetime.Permanent);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
			}

			return builder;
		}

		private static IContainer AddContainer(this IServiceCollection services, string slot)
		{
			if (Containers.TryGetValue(slot, out var container))
				return container;

			var serviceProvider = services.BuildServiceProvider();
			container = new DependencyContainer(serviceProvider);
			container.Register(r => serviceProvider);
			Containers.TryAdd(slot, container);

			return container;
		}

		private static ContainerBuilder AddDatabaseConnection<TConnectionFactory>(this IServiceCollection services,
			string connectionString,
			ConnectionScope scope,
			string slot,
			IEnumerable<IResolverExtension> extensions = null,
			Action<IDbConnection, IServiceProvider> onConnection = null,
			Action<IDbCommand, Type, IServiceProvider> onCommand = null)
			where TConnectionFactory : class, IConnectionFactory, new()
		{
			var factory = new TConnectionFactory {ConnectionString = connectionString};

			services.AddSingleton(factory);

			var container = services.AddContainer(slot);
			container.Register(slot, r => factory, Lifetime.Permanent);

			if (extensions != null)
			{
				foreach (var extension in extensions)
				{
					container.AddExtension(extension);
				}
			}

			switch (scope)
			{
				case ConnectionScope.AlwaysNew:
					container.Register(slot,
						r => new DataContext(r.Resolve<TConnectionFactory>(slot), r.Resolve<IServiceProvider>(),
							onConnection));
					container.Register<IDataConnection>(slot,
						r => new DataConnection(r.Resolve<DataContext>(slot), r.Resolve<IServiceProvider>(),
							onCommand));
					break;
				case ConnectionScope.ByRequest:
					container.Register(slot,
						r => new DataContext(r.Resolve<TConnectionFactory>(slot), r.Resolve<IServiceProvider>(),
							onConnection),
						Lifetime.Request);
					container.Register<IDataConnection>(slot,
						r => new DataConnection(r.Resolve<DataContext>(slot), r.Resolve<IServiceProvider>(), onCommand),
						Lifetime.Request);
					break;
				case ConnectionScope.ByThread:
					container.Register(slot,
						r => new DataContext(r.Resolve<TConnectionFactory>(slot), r.Resolve<IServiceProvider>(),
							onConnection),
						Lifetime.Thread);
					container.Register<IDataConnection>(slot,
						r => new DataConnection(r.Resolve<DataContext>(slot), r.Resolve<IServiceProvider>(), onCommand),
						Lifetime.Thread);
					break;
				case ConnectionScope.KeepAlive:
					container.Register(slot,
						r => new DataContext(r.Resolve<TConnectionFactory>(slot), r.Resolve<IServiceProvider>(),
							onConnection),
						Lifetime.Permanent);
					container.Register<IDataConnection>(slot,
						r => new DataConnection(r.Resolve<DataContext>(slot), r.Resolve<IServiceProvider>(), onCommand),
						Lifetime.Permanent);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
			}

			return new ContainerBuilder(services, container);
		}
	}
}