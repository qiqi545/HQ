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
using System.Data;
using ActiveResolver;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Data.SessionManagement
{
	public static class Add
	{
		private static readonly ConcurrentDictionary<string, DependencyContainer> Containers = new ConcurrentDictionary<string, DependencyContainer>();
		
		public static IServiceCollection AddDatabaseConnection<TScope, TConnectionFactory>(
			this IServiceCollection services, string connectionString, 
			ConnectionScope scope = ConnectionScope.AlwaysNew,
			Action<IDbConnection, IServiceProvider> onConnection = null,
			Action<IDbCommand, Type, IServiceProvider> onCommand = null)
			where TConnectionFactory : class, IConnectionFactory, new()
		{
			var slot = $"{typeof(TScope).FullName}";

			var factory = new TConnectionFactory {ConnectionString = connectionString};
			services.AddSingleton(factory);

			var container = services.AddOrGetContainer(slot);
			container.Register(slot, factory);

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
						b => WrapLifecycle(container, b, Lifetime.Request));
					container.Register<IDataConnection>(slot,
						r => new DataConnection(r.Resolve<DataContext>(slot), r.Resolve<IServiceProvider>(), onCommand),
						b => WrapLifecycle(container, b, Lifetime.Request));
					break;
				case ConnectionScope.ByThread:
					container.Register(slot,
						r => new DataContext(r.Resolve<TConnectionFactory>(slot), r.Resolve<IServiceProvider>(),
							onConnection),
						b => WrapLifecycle(container, b, Lifetime.Thread));
					container.Register<IDataConnection>(slot,
						r => new DataConnection(r.Resolve<DataContext>(slot), r.Resolve<IServiceProvider>(), onCommand),
						b => WrapLifecycle(container, b, Lifetime.Thread));
					break;
				case ConnectionScope.KeepAlive:
					container.Register(slot,
						r => new DataContext(r.Resolve<TConnectionFactory>(slot), r.Resolve<IServiceProvider>(),
							onConnection),
						b => WrapLifecycle(container, b, Lifetime.Permanent));
					container.Register<IDataConnection>(slot,
						r => new DataConnection(r.Resolve<DataContext>(slot), r.Resolve<IServiceProvider>(), onCommand), 
						b => WrapLifecycle(container, b, Lifetime.Permanent));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
			}
			
			if(!TryGetContainer(slot, out container))
				throw new ArgumentException($"Could not initialize container with slot {slot}", slot);

			services.AddTransient(r => container.Resolve<IDataConnection<TScope>>());

			switch (scope)
			{
				case ConnectionScope.AlwaysNew:
					container.Register<IDataConnection<TScope>>(r =>
						new DataConnection<TScope>(r.Resolve<DataContext>(slot), r.Resolve<IServiceProvider>(),
							onCommand));
					break;
				case ConnectionScope.ByRequest:
					container.Register<IDataConnection<TScope>>(
						r => new DataConnection<TScope>(r.Resolve<DataContext>(slot), r.Resolve<IServiceProvider>(),
							onCommand), b => WrapLifecycle(container, b, Lifetime.Request));
					break;
				case ConnectionScope.ByThread:
					container.Register<IDataConnection<TScope>>(
						r => new DataConnection<TScope>(r.Resolve<DataContext>(slot), r.Resolve<IServiceProvider>(),
							onCommand), b => WrapLifecycle(container, b, Lifetime.Thread));
					break;
				case ConnectionScope.KeepAlive:
					container.Register<IDataConnection<TScope>>(
						r => new DataConnection<TScope>(r.Resolve<DataContext>(slot), r.Resolve<IServiceProvider>(),
							onCommand), b => WrapLifecycle(container, b, Lifetime.Permanent));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
			}

			return services;
		}

		private static DependencyContainer AddOrGetContainer(this IServiceCollection services, string slot)
		{
			if (Containers.TryGetValue(slot, out var container))
				return container;

			var serviceProvider = services.BuildServiceProvider();
			container = new DependencyContainer(serviceProvider);
			Containers.TryAdd(slot, container);

			return container;
		}

		private static bool TryGetContainer(string slot, out DependencyContainer container)
		{
			return Containers.TryGetValue(slot, out container);
		}

		private static Func<DependencyContainer, T> WrapLifecycle<T>(DependencyContainer host, Func<DependencyContainer, T> builder, Lifetime lifetime)
			where T : class
		{
			var registration = lifetime switch
			{
				Lifetime.AlwaysNew => builder,
				Lifetime.Permanent => InstanceIsUnique.PerProcess(builder),
				Lifetime.Thread => InstanceIsUnique.PerThread(builder),
				Lifetime.Request => InstanceIsUnique.PerHttpRequest(builder),
				_ => throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "No extensions can serve this lifetime.")
			};

			return registration;
		}

		public enum Lifetime
		{
			AlwaysNew,
			Permanent,
			Thread,
			Request
		}
	}
}