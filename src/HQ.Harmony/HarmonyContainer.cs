// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Harmony
{
	public sealed class HarmonyContainer : IContainer
	{
		private readonly List<IResolverExtension> _extensions;
		private readonly InstanceFactory _factory;
		private readonly IEnumerable<Assembly> _fallbackAssemblies;

		public HarmonyContainer(IEnumerable<Assembly> fallbackAssemblies = null)
		{
			_fallbackAssemblies = fallbackAssemblies ?? Enumerable.Empty<Assembly>();
			_factory = new InstanceFactory();
			_extensions = new List<IResolverExtension>();
		}

		public bool ThrowIfCantResolve { get; set; }

		public void Dispose()
		{
			// No scopes, so nothing to dispose
		}

		internal void AddExtension<T>(T extension) where T : IResolverExtension
		{
			_extensions.Add(extension);
		}

		#region Instancing

		/// <summary> Provides high-performance object activation. </summary>
		public class InstanceFactory
		{
			public delegate object ObjectActivator(params object[] parameters);

			public delegate object ParameterlessObjectActivator();

			private readonly IDictionary<Type, ObjectActivator> _activators =
				new ConcurrentDictionary<Type, ObjectActivator>();

			private readonly IDictionary<ConstructorInfo, ParameterInfo[]> _constructorParameters =
				new ConcurrentDictionary<ConstructorInfo, ParameterInfo[]>();

			private readonly IDictionary<Type, ConstructorInfo> _constructors =
				new ConcurrentDictionary<Type, ConstructorInfo>();

			private readonly IDictionary<Type, ParameterlessObjectActivator> _emptyActivators =
				new ConcurrentDictionary<Type, ParameterlessObjectActivator>();

			/// <summary> Create an instance of the same type as the provided instance. </summary>
			public object CreateInstance(object example)
			{
				return CreateInstance(example.GetType());
			}

			/// <summary> Create a typed instance assuming a parameter-less constructor. </summary>
			public T CreateInstance<T>()
			{
				return (T) CreateInstance(typeof(T));
			}

			public T CreateInstance<T>(object[] args)
			{
				return (T) CreateInstance(typeof(T), args);
			}

			/// <summary> Create an instance of a type assuming a parameter-less constructor. </summary>
			public object CreateInstance(Type implementationType)
			{
				// activator 
				if (_emptyActivators.TryGetValue(implementationType, out var activator))
					return activator();
				var ctor = implementationType.GetConstructor(Type.EmptyTypes);
				_emptyActivators[implementationType] = activator = DynamicMethodFactory.Build(implementationType, ctor);
				return activator();
			}

			/// <summary> Create an instance of a type assuming a set of parameters. </summary>
			public object CreateInstance(Type implementationType, object[] args)
			{
				if (args == null || args.Length == 0)
					return CreateInstance(implementationType);

				// activator 
				if (!_activators.TryGetValue(implementationType, out var activator))
				{
					var ctor = GetOrCacheConstructorForType(implementationType);
					var parameters = GetOrCacheParametersForConstructor(ctor);
					_activators[implementationType] =
						activator = DynamicMethodFactory.Build(implementationType, ctor, parameters);
				}

				return activator(args);
			}

			public ParameterInfo[] GetOrCacheParametersForConstructor(ConstructorInfo ctor)
			{
				// constructor->parameters
				if (!_constructorParameters.TryGetValue(ctor, out var parameters))
					_constructorParameters[ctor] = parameters = ctor.GetParameters();
				return parameters;
			}

			public ConstructorInfo GetOrCacheConstructorForType(Type implementationType)
			{
				// type->constructor
				if (!_constructors.TryGetValue(implementationType, out var ctor))
					_constructors[implementationType] = ctor = GetWidestConstructor(implementationType);
				return ctor;
			}

			private static ConstructorInfo GetWidestConstructor(Type implementationType)
			{
				var ctors = implementationType.GetConstructors();
				var ctor = ctors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
				return ctor ?? implementationType.GetConstructor(Type.EmptyTypes);
			}

			/// <summary>
			///     Source:
			///     <see
			///         cref="http://stackoverflow.com/questions/2353174/c-sharp-emitting-dynamic-method-delegate-to-load-parametrized-constructor-proble" />
			/// </summary>
			private static class DynamicMethodFactory
			{
				public static ParameterlessObjectActivator Build(Type implementationType, ConstructorInfo ctor)
				{
					var dynamicMethod = new DynamicMethod($"{implementationType.FullName}.0ctor", implementationType,
						Type.EmptyTypes, true);
					var il = dynamicMethod.GetILGenerator();
					il.Emit(OpCodes.Nop);
					il.Emit(OpCodes.Newobj, ctor);
					il.Emit(OpCodes.Ret);
					return (ParameterlessObjectActivator) dynamicMethod.CreateDelegate(
						typeof(ParameterlessObjectActivator));
				}

				public static ObjectActivator Build(Type implementationType, ConstructorInfo ctor,
					IReadOnlyList<ParameterInfo> parameters)
				{
					var dynamicMethod = new DynamicMethod($"{implementationType.FullName}.ctor", implementationType,
						new[] {typeof(object[])});
					var il = dynamicMethod.GetILGenerator();
					for (var i = 0; i < parameters.Count; i++)
					{
						il.Emit(OpCodes.Ldarg_0);
						switch (i)
						{
							case 0:
								il.Emit(OpCodes.Ldc_I4_0);
								break;
							case 1:
								il.Emit(OpCodes.Ldc_I4_1);
								break;
							case 2:
								il.Emit(OpCodes.Ldc_I4_2);
								break;
							case 3:
								il.Emit(OpCodes.Ldc_I4_3);
								break;
							case 4:
								il.Emit(OpCodes.Ldc_I4_4);
								break;
							case 5:
								il.Emit(OpCodes.Ldc_I4_5);
								break;
							case 6:
								il.Emit(OpCodes.Ldc_I4_6);
								break;
							case 7:
								il.Emit(OpCodes.Ldc_I4_7);
								break;
							case 8:
								il.Emit(OpCodes.Ldc_I4_8);
								break;
							default:
								il.Emit(OpCodes.Ldc_I4, i);
								break;
						}

						il.Emit(OpCodes.Ldelem_Ref);
						var paramType = parameters[i].ParameterType;
						il.Emit(paramType.GetTypeInfo().IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, paramType);
					}

					il.Emit(OpCodes.Newobj, ctor);
					il.Emit(OpCodes.Ret);
					return (ObjectActivator) dynamicMethod.CreateDelegate(typeof(ObjectActivator));
				}
			}
		}

		#endregion

		#region Register

		private readonly IDictionary<Type, Func<object>>
			_registrations = new ConcurrentDictionary<Type, Func<object>>();

		private readonly IDictionary<NameAndType, Func<object>> _namedRegistrations =
			new ConcurrentDictionary<NameAndType, Func<object>>();

		private readonly IDictionary<Type, List<Func<object>>> _collectionRegistrations =
			new ConcurrentDictionary<Type, List<Func<object>>>();

		public void Register(Type type, Func<object> builder, Lifetime lifetime = Lifetime.AlwaysNew)
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
		}

		public void Register<T>(Func<T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class
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
		}

		public void Register<T>(string name, Func<T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class
		{
			var type = typeof(T);
			_namedRegistrations[new NameAndType(name, type)] = WrapLifecycle(builder, lifetime);
		}

		public void Register<T>(string name, Func<IDependencyResolver, T> builder,
			Lifetime lifetime = Lifetime.AlwaysNew) where T : class
		{
			var registration = WrapLifecycle(builder, lifetime);
			_namedRegistrations[new NameAndType(name, typeof(T))] = () => registration(this);
		}

		public void Register<T>(Func<IDependencyResolver, T> builder, Lifetime lifetime = Lifetime.AlwaysNew)
			where T : class
		{
			var type = typeof(T);
			Func<object> next = () => WrapLifecycle(builder, lifetime)(this);
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
		}

		public void Register<T>(T instance)
		{
			var type = typeof(T);
			Func<object> next = () => instance;
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
				var collection = (IList) _factory.CreateInstance(typeof(List<>).MakeGenericType(type));
				foreach (var item in YieldCollection(collectionBuilder))
					collection.Add(item);
				return collection;
			}, Lifetime.Permanent);
		}

		#endregion

		#region Resolve

		public T Resolve<T>() where T : class
		{
			var serviceType = typeof(T);
			if (!_registrations.TryGetValue(serviceType, out var builder))
				return AutoResolve(serviceType) as T;
			if (builder() is T resolved)
				return resolved;
			if (ThrowIfCantResolve)
				throw new InvalidOperationException($"No registration for {serviceType}");
			return null;
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

		public object Resolve(Type serviceType)
		{
			if (!_registrations.TryGetValue(serviceType, out var builder))
				return AutoResolve(serviceType);
			var resolved = builder();
			if (resolved != null)
				return resolved;
			if (ThrowIfCantResolve)
				throw new InvalidOperationException($"No registration for {serviceType}");
			return null;
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

		public T Resolve<T>(string name) where T : class
		{
			if (_namedRegistrations.TryGetValue(new NameAndType(name, typeof(T)), out var builder))
				return builder() as T;
			if (ThrowIfCantResolve)
				throw new InvalidOperationException($"No registration for {typeof(T)} named {name}");
			return null;
		}

		public object Resolve(string name, Type serviceType)
		{
			if (_namedRegistrations.TryGetValue(new NameAndType(name, serviceType), out var builder))
				return builder();
			if (ThrowIfCantResolve)
				throw new InvalidOperationException($"No registration for {serviceType} named {name}");
			return null;
		}

		#endregion

		#region Auto-Resolve w/ Fallback

		private object CreateInstance(Type implementationType)
		{
			// type->constructor
			var ctor = _factory.GetOrCacheConstructorForType(implementationType);

			// constructor->parameters
			var parameters = _factory.GetOrCacheParametersForConstructor(ctor);

			// parameterless ctor
			if (parameters.Length == 0)
				return _factory.CreateInstance(implementationType);

			// auto-resolve widest ctor
			var args = new object[parameters.Length];
			for (var i = 0; i < parameters.Length; i++)
				args[i] = AutoResolve(parameters[i].ParameterType);

			return _factory.CreateInstance(implementationType, args);
		}

		public object AutoResolve(Type serviceType)
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
				var type = _fallbackAssemblies.SelectMany(s => s.GetTypes())
					.FirstOrDefault(i => serviceType.IsAssignableFrom(i) && !i.GetTypeInfo().IsInterface);
				if (type == null)
				{
					if (ThrowIfCantResolve)
						throw new InvalidOperationException($"No registration for {serviceType}");

					return null;
				}

				serviceType = type;
			}
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
#if SupportsRequests
                case Lifetime.Request:
                    registration = RequestMemoize(builder);
                    break;
#endif
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