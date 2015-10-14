using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using container.Default;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace container
{
    public class Container : IContainer, IServiceProvider
    {
        private readonly IDependencyResolver _resolver;
        private readonly IDependencyRegistrar _registrar;

        public Container()
        {
            var container = new DefaultContainer();
            _resolver = container;
            _registrar = container;
            LoadedAssemblies = Enumerable.Empty<Assembly>();
        }

        public Container(IServiceCollection services, ILogger logger)
        {
            LoadedAssemblies = LoadAllAssemblies(services, logger);
            var container = new DefaultContainer(LoadedAssemblies);
            container.Register(logger);
            _resolver = container;
            _registrar = container;
        }

        #region Resolver

        public T Resolve<T>() where T : class
        {
            return _resolver.Resolve<T>();
        }

        public T Resolve<T>(string name) where T : class
        {
            return _resolver.Resolve<T>(name);
        }

        public object Resolve(Type serviceType)
        {
            return _resolver.Resolve(serviceType);
        }

        public object Resolve(Type serviceType, string name)
        {
            return _resolver.Resolve(serviceType, name);
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return _resolver.ResolveAll<T>();
        }

        #endregion


        #region Registrar

        public void Register<T>(Func<T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class
        {
            _registrar.Register(builder, lifetime);
        }

        public void Register<T>(string name, Func<T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class
        {
            _registrar.Register(name, builder, lifetime);
        }

        public void Register<T>(Func<IDependencyResolver, T> builder, Lifetime lifetime = Lifetime.AlwaysNew)
            where T : class
        {
            _registrar.Register(r => builder(this), lifetime);
        }

        public void Register<T>(string name, Func<IDependencyResolver, T> builder,
            Lifetime lifetime = Lifetime.AlwaysNew) where T : class
        {
            _registrar.Register(name, builder, lifetime);
        }

        public void Register(Type type, Func<IDependencyResolver, object> builder, Lifetime lifetime = Lifetime.AlwaysNew)
        {
            _registrar.Register(type, builder, lifetime);
        }
        public void Register(Type interfaceType, Type implementationType, Lifetime lifetime)
        {
            _registrar.Register(interfaceType, implementationType, lifetime);
        }
        public void Register<T>(T instance)
        {
            _registrar.Register(instance);
        }

        #endregion

        public void Dispose()
        {
            _resolver.Dispose();
            if (_registrar != _resolver)
            {
                _registrar.Dispose();
            }
        }

        public object GetService(Type serviceType)
        {
            return _resolver.Resolve(serviceType);
        }

        private static IEnumerable<Assembly> LoadAllAssemblies(IServiceCollection services, ILogger logger)
        {
            var assemblies = new List<Assembly>();
            var provider = services.BuildServiceProvider();
            var libraryManager = provider.GetService<ILibraryManager>();
            var libraries = libraryManager.GetLibraries();

            foreach (Library library in libraries)
            {
                foreach (var assemblyName in library.Assemblies)
                {
                    try
                    {
                        assemblies.Add(Assembly.Load(assemblyName));
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Verbose, 500, assemblyName, ex, (o, exception) => "Could not load assembly during startup: " + o);
                    }
                }
            }

            return assemblies;
        }

        public IEnumerable<Assembly> LoadedAssemblies { get; }
    }
}
 