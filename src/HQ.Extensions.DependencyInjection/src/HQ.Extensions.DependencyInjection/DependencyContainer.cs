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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using HQ.Extensions.CodeGeneration;
using HQ.Extensions.DependencyInjection.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Extensions.DependencyInjection
{
    public sealed class DependencyContainer : IContainer, IMethodResolver, IMethodInvoker
    {
        private readonly List<IResolverExtension> _extensions;
        private readonly IEnumerable<Assembly> _fallbackAssemblies;
        private readonly IServiceProvider _fallbackProvider;
        private readonly IMethodInvoker _methodInvoker;

        private readonly IMethodResolver _methodResolver;

        public DependencyContainer(
            IServiceProvider fallbackProvider = null,
            IEnumerable<Assembly> fallbackAssemblies = null,
            IMethodResolver methodResolver = null,
            IMethodInvoker methodInvoker = null)
        {
            _extensions = new List<IResolverExtension>();

            _fallbackProvider = fallbackProvider;
            _fallbackAssemblies = fallbackAssemblies ?? Enumerable.Empty<Assembly>();
            _methodResolver = methodResolver ?? new DefaultMethodResolver(this);
            _methodInvoker = methodInvoker ?? new DefaultMethodInvoker(this);
        }

        public bool ThrowIfCantResolve { get; set; }

        public void Dispose() { }

        internal bool AddExtension<T>(T extension) where T : IResolverExtension
        {
            if (_extensions.Contains(extension))
                return false;
            _extensions.Add(extension);
            return true;
        }

        #region Method Resolution

        public MethodInfo ResolveMethod(Type serviceType, string name)
        {
            return _methodResolver?.ResolveMethod(serviceType, name);
        }

        public MethodInfo ResolveMethod<T>(string name) where T : class
        {
            return _methodResolver?.ResolveMethod<T>(name);
        }

        public MethodInfo ResolveMethod(string serviceTypeName, string name)
        {
            return _methodResolver?.ResolveMethod(serviceTypeName, name);
        }

        public object InvokeMethod(Type serviceType, string name)
        {
            return _methodInvoker.InvokeMethod(serviceType, name);
        }

        public object InvokeMethod<T>(string name) where T : class
        {
            return _methodInvoker.InvokeMethod<T>(name);
        }

        #endregion

        #region Register

        private readonly IDictionary<Type, Func<object>>
            _registrations = new ConcurrentDictionary<Type, Func<object>>();

        private readonly IDictionary<NameAndType, Func<object>> _namedRegistrations =
            new ConcurrentDictionary<NameAndType, Func<object>>();

        private readonly IDictionary<Type, List<Func<object>>> _collectionRegistrations =
            new ConcurrentDictionary<Type, List<Func<object>>>();

        public IDependencyRegistrar Register(Type type, Func<object> builder, Lifetime lifetime = Lifetime.AlwaysNew)
        {
            var next = WrapLifecycle(builder, lifetime);
            if (_registrations.ContainsKey(type))
            {
                var previous = _registrations[type];
                _registrations[type] = next;
                RegisterManyUnnamed(type, previous);
            }
            else
            {
                _registrations[type] = next;
            }

            return this;
        }

        public IDependencyRegistrar Register<T>(Func<T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class
        {
            var type = typeof(T);
            Func<object> next = WrapLifecycle(builder, lifetime);
            if (_registrations.ContainsKey(type))
            {
                var previous = _registrations[type];
                _registrations[type] = next;
                RegisterManyUnnamed(type, previous);
            }
            else
            {
                _registrations[type] = next;
            }

            return this;
        }

        public IDependencyRegistrar Register<T>(string name, Func<T> builder, Lifetime lifetime = Lifetime.AlwaysNew)
            where T : class
        {
            var type = typeof(T);
            _namedRegistrations[new NameAndType(name, type)] = WrapLifecycle(builder, lifetime);
            return this;
        }

        public IDependencyRegistrar Register<T>(string name, Func<IDependencyResolver, T> builder,
            Lifetime lifetime = Lifetime.AlwaysNew) where T : class
        {
            var registration = WrapLifecycle(builder, lifetime);
            _namedRegistrations[new NameAndType(name, typeof(T))] = () => registration(this);
            return this;
        }

        public IDependencyRegistrar Register<T>(Func<IDependencyResolver, T> builder,
            Lifetime lifetime = Lifetime.AlwaysNew)
            where T : class
        {
            var type = typeof(T);
            if (_registrations.ContainsKey(type))
            {
                var previous = _registrations[type];
                _registrations[type] = () => WrapLifecycle(builder, lifetime)(this);
                RegisterManyUnnamed(type, previous);
            }
            else
            {
                _registrations[type] = () => WrapLifecycle(builder, lifetime)(this);
            }

            return this;
        }

        public IDependencyRegistrar Register<T>(T instance)
        {
            return Register(typeof(T), instance);
        }

        public IDependencyRegistrar Register(object instance)
        {
            var type = instance.GetType();
            return Register(type, instance);
        }

        private IDependencyRegistrar Register(Type type, object instance)
        {
            if (_registrations.ContainsKey(type))
            {
                var previous = _registrations[type];
                _registrations[type] = () => instance;
                RegisterManyUnnamed(type, previous);
            }
            else
            {
                _registrations[type] = () => instance;
            }

            return this;
        }

        public bool TryRegister<T>(T instance)
        {
            var type = typeof(T);
            if (_registrations.ContainsKey(type))
                return false;
            _registrations[type] = () => instance;
            return true;
        }

        public bool TryRegister(object instance)
        {
            var type = instance.GetType();
            if (_registrations.ContainsKey(type))
                return false;
            _registrations[type] = () => instance;
            return true;
        }

        private void RegisterManyUnnamed(Type type, Func<object> previous)
        {
            if (!_collectionRegistrations.TryGetValue(type, out var collectionBuilder))
            {
                collectionBuilder = new List<Func<object>> {previous};
                _collectionRegistrations.Add(type, collectionBuilder);
            }

            collectionBuilder.Add(_registrations[type]);

            // implied registration of the enumerable equivalent
            Register(typeof(IEnumerable<>).MakeGenericType(type), () =>
            {
                var collection = (IList) InstanceFactory.Default.CreateInstance(typeof(List<>).MakeGenericType(type));
                foreach (var item in YieldCollection(collectionBuilder))
                    collection.Add(item);
                return collection;
            }, Lifetime.Permanent);
        }

        #endregion

        #region Resolve

        public T Resolve<T>() where T : class
        {
            return ResolveTyped<T>(ThrowIfCantResolve);
        }

        public T MustResolve<T>() where T : class
        {
            return ResolveTyped<T>(true);
        }

        private T ResolveTyped<T>(bool throwIfCantResolve) where T : class
        {
            var serviceType = typeof(T);
            if (!_registrations.TryGetValue(serviceType, out var builder))
                return AutoResolve(serviceType, throwIfCantResolve) as T;
            return TryTypedResolve<T>(throwIfCantResolve, builder);
        }
        
        public object Resolve(Type serviceType)
        {
            return ResolveUntyped(serviceType, ThrowIfCantResolve);
        }

        public object MustResolve(Type serviceType)
        {
            return ResolveUntyped(serviceType, true);
        }

        private object ResolveUntyped(Type serviceType, bool throwIfCantResolve)
        {
            return !_registrations.TryGetValue(serviceType, out var builder)
                ? AutoResolve(serviceType, throwIfCantResolve)
                : TryUntypedResolve(serviceType, throwIfCantResolve, builder);
        }

        public T Resolve<T>(string name) where T : class
        {
            return ResolveNamedTyped<T>(name, ThrowIfCantResolve);
        }

        public T MustResolve<T>(string name) where T : class
        {
            return ResolveNamedTyped<T>(name, true);
        }

        private T ResolveNamedTyped<T>(string name, bool throwIfCantResolve) where T : class
        {
            if (_namedRegistrations.TryGetValue(new NameAndType(name, typeof(T)), out var builder))
                return TryTypedResolve<T>(throwIfCantResolve, builder);
            if (throwIfCantResolve)
                throw new InvalidOperationException($"No registration for {typeof(T)} named {name}");
            return null;
        }

        public object Resolve(string name, Type serviceType)
        {
            return ResolveNamedUntyped(name, serviceType, ThrowIfCantResolve);
        }

        public object MustResolve(string name, Type serviceType)
        {
            return ResolveNamedUntyped(name, serviceType, true);
        }

        private object ResolveNamedUntyped(string name, Type serviceType, bool throwIfCantResolve)
        {
            if (_namedRegistrations.TryGetValue(new NameAndType(name, serviceType), out var builder))
                return TryUntypedResolve(serviceType, throwIfCantResolve, builder);
            if (throwIfCantResolve)
                throw new InvalidOperationException($"No registration for {serviceType} named {name}");
            return null;
        }

        private static T TryTypedResolve<T>(bool throwIfCantResolve, Func<object> builder) where T : class
        {
            switch (builder())
            {
                case T resolved:
                    return resolved;
                case null when throwIfCantResolve:
                    throw new InvalidOperationException($"No registration for {typeof(T)}");
                default:
                    return null;
            }
        }

        private static object TryUntypedResolve(Type serviceType, bool throwIfCantResolve, Func<object> builder)
        {
            switch (builder())
            {
                case object resolved:
                    return resolved;
                case null when throwIfCantResolve:
                    throw new InvalidOperationException($"No registration for {serviceType}");
                default:
                    return null;
            }
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            var serviceType = typeof(T);
            return _collectionRegistrations.TryGetValue(serviceType, out var collectionBuilder)
                ? YieldCollection<T>(collectionBuilder)
                : Enumerable.Empty<T>();
        }

        private static IEnumerable<T> YieldCollection<T>(IEnumerable<Func<object>> collectionBuilder) where T : class
        {
            return collectionBuilder.Select(builder => builder() as T);
        }

        public IEnumerable ResolveAll(Type serviceType)
        {
            return _collectionRegistrations.TryGetValue(serviceType, out var collectionBuilder)
                ? YieldCollection(collectionBuilder)
                : Enumerable.Empty<object>();
        }

        private static IEnumerable YieldCollection(IEnumerable<Func<object>> collectionBuilder)
        {
            return collectionBuilder.Select(builder => builder());
        }

        #endregion

        #region Auto-Resolve w/ Fallback

        private object AutoResolve(Type serviceType, bool throwIfCantResolve)
        {
            while (true)
            {
                // got it:
                if (_registrations.TryGetValue(serviceType, out var creator))
                    return creator();

                // want it:
                var typeInfo = serviceType.GetTypeInfo();
                if (!typeInfo.IsAbstract)
                    return CreateInstance(serviceType);

                // need it:
                var fallback = _fallbackProvider?.GetService(serviceType);
                if (fallback != null)
                    return fallback;

                var type = _fallbackAssemblies.SelectMany(s => s.GetTypes())
                    .FirstOrDefault(i => serviceType.IsAssignableFrom(i) && !i.GetTypeInfo().IsInterface);

                if (type == null)
                {
                    if (throwIfCantResolve)
                        throw new InvalidOperationException($"No registration for {serviceType}");
                    return null;
                }

                serviceType = type;
            }
        }

        private object CreateInstance(Type implementationType)
        {
            // type->constructor
            var ctor = InstanceFactory.Default.GetOrCacheConstructorForType(implementationType);

            // constructor->parameters
            var parameters = InstanceFactory.Default.GetOrCacheParametersForConstructor(ctor);

            // parameterless ctor
            if (parameters.Length == 0)
                return InstanceFactory.Default.CreateInstance(implementationType);

            // auto-resolve widest ctor
            var args = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
                args[i] = AutoResolve(parameters[i].ParameterType, ThrowIfCantResolve);

            return InstanceFactory.Default.CreateInstance(implementationType, args);
        }

        #endregion

        #region Lifetime Management

        private Func<IDependencyResolver, T> WrapLifecycle<T>(Func<IDependencyResolver, T> builder, Lifetime lifetime)
            where T : class
        {
            Func<IDependencyResolver, T> registration;
            switch (lifetime)
            {
                case Lifetime.AlwaysNew:
                    registration = builder;
                    break;
                case Lifetime.Permanent:
                    registration = ProcessMemoize(builder);
                    break;
                case Lifetime.Thread:
                    registration = ThreadMemoize(builder);
                    break;
                case Lifetime.Request:
                    foreach (var extension in _extensions)
                        if (extension.CanResolve(lifetime))
                            return extension.Memoize(this, builder);
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime,
                        "No extensions can serve this lifetime.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }

            return registration;
        }

        private Func<T> WrapLifecycle<T>(Func<T> builder, Lifetime lifetime) where T : class
        {
            Func<T> registration;
            switch (lifetime)
            {
                case Lifetime.AlwaysNew:
                    registration = builder;
                    break;
                case Lifetime.Permanent:
                    registration = ProcessMemoize(builder);
                    break;
                case Lifetime.Thread:
                    registration = ThreadMemoize(builder);
                    break;
                case Lifetime.Request:
                    foreach (var extension in _extensions)
                        if (extension.CanResolve(lifetime))
                            return extension.Memoize(this, builder);
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime,
                        "No extensions can serve this lifetime.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }

            return registration;
        }

        private static Func<T> ProcessMemoize<T>(Func<T> f)
        {
            var cache = new ConcurrentDictionary<Type, T>();

            return () => cache.GetOrAdd(typeof(T), v => f());
        }

        private static Func<T> ThreadMemoize<T>(Func<T> f)
        {
            var cache = new ThreadLocal<T>(f);

            return () => cache.Value;
        }

        private Func<IDependencyResolver, T> ProcessMemoize<T>(Func<IDependencyResolver, T> f)
        {
            var cache = new ConcurrentDictionary<Type, T>();

            return r => cache.GetOrAdd(typeof(T), v => f(this));
        }

        private Func<IDependencyResolver, T> ThreadMemoize<T>(Func<IDependencyResolver, T> f)
        {
            var cache = new ThreadLocal<T>(() => f(this));

            return r => cache.Value;
        }

        #endregion

        #region Scoping Features

        public IServiceProvider Populate(IServiceCollection services)
        {
            Register<IServiceProvider>(() => new NoServiceProvider(this, services), Lifetime.Permanent);
            Register<IServiceScopeFactory>(() => new NoServiceScopeFactory(this), Lifetime.Permanent);
            Register<IEnumerable<ServiceDescriptor>>(services);
            Register(this);
            return Resolve<IServiceProvider>();
        }

        internal sealed class NoServiceScopeFactory : IServiceScopeFactory
        {
            private readonly IContainer _container;

            public NoServiceScopeFactory(IContainer container)
            {
                _container = container;
            }

            public IServiceScope CreateScope()
            {
                return new NoServiceScope(_container);
            }

            private class NoServiceScope : IServiceScope
            {
                private readonly IContainer _container;

                public NoServiceScope(IContainer container)
                {
                    _container = container;
                }

                public IServiceProvider ServiceProvider => _container.Resolve<IServiceProvider>();

                public void Dispose()
                {
                    _container.Dispose();
                }
            }
        }

        internal sealed class NoServiceProvider : IServiceProvider, ISupportRequiredService
        {
            private readonly IContainer _container;
            private readonly IServiceProvider _fallback;

            public NoServiceProvider(IContainer container, IServiceCollection services)
            {
                _container = container;
                _fallback = services.BuildServiceProvider();
                RegisterServiceDescriptors(services);
            }

            public object GetService(Type serviceType)
            {
                return _container.Resolve(serviceType) ?? _fallback.GetService(serviceType);
            }

            public object GetRequiredService(Type serviceType)
            {
                return _container.Resolve(serviceType) ?? _fallback.GetRequiredService(serviceType);
            }

            private void RegisterServiceDescriptors(IServiceCollection services)
            {
                // we're going to shell out to the native container for anything passed in here
                foreach (var descriptor in services)
                    _container.Register(descriptor.ServiceType, () => _fallback.GetService(descriptor.ServiceType));
            }
        }

        #endregion
    }
}
