using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DryIoc;

namespace container.Default
{
    public class DefaultContainer : IContainer
    {
        private readonly DryIoc.IContainer _container;
        public DefaultContainer()
        {
            _container = new DryIoc.Container();
            LoadedAssemblies = Enumerable.Empty<Assembly>();
        }
        public DefaultContainer(IEnumerable<Assembly> assemblies)
        {
            LoadedAssemblies = assemblies;
            _container = new DryIoc.Container().WithAutoFallbackResolution(assemblies);
        }
        public T Resolve<T>() where T : class
        {
            return _container.Resolve<T>();
        }
        public T Resolve<T>(string name) where T : class
        {
            return _container.Resolve<T>(name, IfUnresolved.ReturnDefault);
        }
        public object Resolve(Type serviceType)
        {
            return _container.Resolve(serviceType, IfUnresolved.ReturnDefault);
        }
        public object Resolve(Type serviceType, string name)
        {
            return _container.Resolve(serviceType, name, IfUnresolved.ReturnDefault);
        }
        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return _container.ResolveMany<T>();
        }
        public void Register<T>(Func<T> builder, Lifetime lifetime) where T : class
        {
            var reuse = GetReuse(lifetime);
            _container.RegisterDelegate(resolver => builder(), reuse);
        }
        public void Register<T>(string name, Func<T> builder, Lifetime lifetime) where T : class
        {
            var reuse = GetReuse(lifetime);
            _container.RegisterDelegate(resolver => builder(), serviceKey: name, reuse: reuse);
        }
        public void Register<T>(Func<IDependencyResolver, T> builder, Lifetime lifetime) where T : class
        {
            var reuse = GetReuse(lifetime);
            _container.RegisterDelegate(resolver => builder(this), reuse);
        }
        public void Register<T>(string name, Func<IDependencyResolver, T> builder, Lifetime lifetime) where T : class
        {
            var reuse = GetReuse(lifetime);
            _container.RegisterDelegate(resolver => builder(this), reuse, serviceKey: name);
        }
        public void Register(Type type, Func<IDependencyResolver, object> builder, Lifetime lifetime)
        {
            var reuse = GetReuse(lifetime);
            _container.RegisterDelegate(type, resolver => builder(this), reuse);
        }
        public void Register(Type interfaceType, Type implementationType, Lifetime lifetime)
        {
            var reuse = GetReuse(lifetime);
            _container.Register(interfaceType, implementationType, reuse);
        }
        public void Register<T>(T instance)
        {
            _container.RegisterDelegate(typeof(T), r => instance, GetReuse(Lifetime.Permanent));
        }
        public void Dispose()
        {
            _container.Dispose();
        }
        private static IReuse GetReuse(Lifetime lifetime)
        {
            IReuse reuse;
            switch (lifetime)
            {
                case Lifetime.AlwaysNew:
                    reuse = Reuse.Transient;
                    break;
                case Lifetime.Permanent:
                    reuse = Reuse.Singleton;
                    break;
                case Lifetime.Thread:
                    reuse = Reuse.InThread;
                    break;
                case Lifetime.Request:
                    reuse = Reuse.InWebRequest;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }
            return reuse;
        }
        public IEnumerable<Assembly> LoadedAssemblies { get; }
    }
}