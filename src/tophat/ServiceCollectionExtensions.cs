using System;
using Microsoft.Extensions.DependencyInjection;

namespace tophat
{
    public static class ServiceCollectionExtensions
    {
		private static readonly NoContainer Container = new NoContainer();

		public static IServiceCollection AddDatabaseConnection<T>(this IServiceCollection services, string connectionString, ConnectionScope scope) where T : class, IConnectionFactory, new()
	    {
		    return AddDatabaseConnection<T>(services, connectionString, scope, "Default");
	    }

	    public static IServiceCollection AddDatabaseConnection<T>(this IServiceCollection services, string connectionString, ConnectionScope scope, string slot) where T : class, IConnectionFactory, new()
	    {
		    Container.Register(slot, r => new T { ConnectionString = connectionString }, Lifetime.Permanent);

		    switch (scope)
		    {
			    case ConnectionScope.AlwaysNew:
				    Container.Register(slot, r => new DataContext(r.Resolve<T>(slot)));
				    break;
			    case ConnectionScope.ByRequest:
				    Container.Register(slot, r => new DataContext(r.Resolve<T>(slot)), Lifetime.Request);
				    break;
			    case ConnectionScope.ByThread:
				    Container.Register(slot, r => new DataContext(r.Resolve<T>(slot)), Lifetime.Thread);
				    break;
			    case ConnectionScope.KeepAlive:
				    Container.Register(slot, r => new DataContext(r.Resolve<T>(slot)), Lifetime.Permanent);
				    break;
			    default:
				    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
		    }

		    services.AddTransient(r => Container.Resolve<DataContext>(slot));
		    return services;
	    }

		public static IServiceCollection AddDatabaseConnection<TScope, TConnectionFactory>(this IServiceCollection services, string connectionString, ConnectionScope scope) 
			where TConnectionFactory : class, IConnectionFactory, new()
		{
			var slot = $"{typeof(TScope).FullName}";

			AddDatabaseConnection<TConnectionFactory>(services, connectionString, scope, slot);

			switch (scope)
			{
				case ConnectionScope.AlwaysNew:
					Container.Register<IDataConnection<TScope>>(r => new DataConnection<TScope>(r.Resolve<DataContext>(slot)));
					break;
				case ConnectionScope.ByRequest:
					Container.Register<IDataConnection<TScope>>(r => new DataConnection<TScope>(r.Resolve<DataContext>(slot)), Lifetime.Request);
					break;
				case ConnectionScope.ByThread:
					Container.Register<IDataConnection<TScope>>(r => new DataConnection<TScope>(r.Resolve<DataContext>(slot)), Lifetime.Thread);
					break;
				case ConnectionScope.KeepAlive:
					Container.Register<IDataConnection<TScope>>(r => new DataConnection<TScope>(r.Resolve<DataContext>(slot)), Lifetime.Permanent);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
			}

			services.AddTransient(r => Container.Resolve<IDataConnection<TScope>>());

			return services; 
		}

		public static IServiceCollection AddDatabaseConnection<T>(this IServiceCollection services, string connectionString, Func<IConnectionFactory, DataContext> scope)
		    where T : class, IConnectionFactory, new()
		{
			return AddDatabaseConnection<T>(services, connectionString, scope, "Default");
		}

	    public static IServiceCollection AddDatabaseConnection<T>(IServiceCollection services, string connectionString, Func<IConnectionFactory, DataContext> scope, string slot)
		    where T : class, IConnectionFactory, new()
	    {
		    Container.Register(slot, r => new T { ConnectionString = connectionString }, Lifetime.Permanent);
			Container.Register(slot, r => scope(r.Resolve<IConnectionFactory>()));		    
		    services.AddTransient(r => Container.Resolve<DataContext>(slot));
		    return services;
	    }
    }
}