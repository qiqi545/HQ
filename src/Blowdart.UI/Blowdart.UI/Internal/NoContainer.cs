#region License
/*
NoContainer - Because, no.
--------------------------

Copyright(c) 2016-2017 Daniel Crenna

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
documentation files (the "Software"), to deal in the Software without restriction, including without limitation
the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

#if ASPNETCORE
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
#define SupportsRequestMemoization
#endif

namespace Blowdart.UI.Internal
{
    internal class NoContainer : IContainer, IServiceProvider
    {
        private readonly IServiceProvider _fallbackProvider;
        private readonly IEnumerable<Assembly> _fallbackAssemblies;

        public bool ThrowIfCantResolve { get; set; }

        public NoContainer(IServiceProvider fallbackProvider = null, IEnumerable<Assembly> fallbackAssemblies = null)
        {
            _fallbackProvider = fallbackProvider;
            _fallbackAssemblies = (fallbackAssemblies ?? Enumerable.Empty<Assembly>()).Distinct();
        }

        #region Register

        public struct NameAndType
        {
            public readonly Type Type;
            public readonly string Name;

            public NameAndType(string name, Type type)
            {
                Name = name;
                Type = type;
            }

            public bool Equals(NameAndType other)
            {
                return Type == other.Type && string.Equals(Name, other.Name);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is NameAndType && Equals((NameAndType)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Type?.GetHashCode() ?? 0) * 397) ^ (Name?.GetHashCode() ?? 0);
                }
            }

            private sealed class TypeNameEqualityComparer : IEqualityComparer<NameAndType>
            {
                public bool Equals(NameAndType x, NameAndType y)
                {
                    return x.Type == y.Type && string.Equals(x.Name, y.Name);
                }

                public int GetHashCode(NameAndType obj)
                {
                    unchecked
                    {
                        return ((obj.Type?.GetHashCode() ?? 0) * 397) ^ (obj.Name?.GetHashCode() ?? 0);
                    }
                }
            }

            public static IEqualityComparer<NameAndType> TypeNameComparer { get; } = new TypeNameEqualityComparer();
        }
        private readonly IDictionary<Type, Func<object>> _registrations = new ConcurrentDictionary<Type, Func<object>>();
        private readonly IDictionary<NameAndType, Func<object>> _namedRegistrations = new ConcurrentDictionary<NameAndType, Func<object>>();

        public void Register<T>(Func<T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class
        {
            Func<T> registration = WrapLifecycle(builder, lifetime);
            _registrations[typeof(T)] = registration;
        }

        public void Register<T>(string name, Func<T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class
        {
            Func<T> registration = WrapLifecycle(builder, lifetime);
            _namedRegistrations[new NameAndType(name, typeof(T))] = registration;
        }

        public void Register<T>(string name, Func<IDependencyResolver, T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class
        {
            Func<IDependencyResolver, T> registration = WrapLifecycle(builder, lifetime);
            _namedRegistrations[new NameAndType(name, typeof(T))] = () => registration(this);
        }

        public void Register<T>(Func<IDependencyResolver, T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class
        {
            Func<IDependencyResolver, T> registration = WrapLifecycle(builder, lifetime);
            _registrations[typeof(T)] = () => registration(this);
        }

        public void Register<T>(T instance)
        {
            _registrations[typeof(T)] = () => instance;
        }

        #endregion

        #region Resolve

        public T Resolve<T>() where T : class
        {
            if (!_registrations.TryGetValue(typeof(T), out var builder))
                return AutoResolve(typeof(T)) as T;
            if (builder() is T resolved)
                return resolved;
            if (ThrowIfCantResolve)
                throw new InvalidOperationException($"No registration for {typeof(T)}");
            return null;
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

        public T Resolve<T>(string name) where T : class
        {
            if (_namedRegistrations.TryGetValue(new NameAndType(name, typeof(T)), out var builder))
                return builder() as T;
            if (ThrowIfCantResolve)
                throw new InvalidOperationException($"No registration for {typeof(T)} named {name}");
            return null;
        }

        public object Resolve(Type serviceType, string name)
        {
            if (_namedRegistrations.TryGetValue(new NameAndType(name, serviceType), out var builder))
                return builder();
            if (ThrowIfCantResolve)
                throw new InvalidOperationException($"No registration for {serviceType} named {name}");
            return null;
        }

        #endregion

        #region Auto-Resolve w/ Fallback

        private readonly IDictionary<Type, ObjectActivator> _activators = new ConcurrentDictionary<Type, ObjectActivator>();
        private readonly IDictionary<Type, ConstructorInfo> _constructors = new ConcurrentDictionary<Type, ConstructorInfo>();
        private readonly IDictionary<ConstructorInfo, ParameterInfo[]> _constructorParameters = new ConcurrentDictionary<ConstructorInfo, ParameterInfo[]>();

        public object AutoResolve(Type serviceType)
        {
            // got it:
            if (_registrations.TryGetValue(serviceType, out var creator))
                return creator();

            // want it:
            var typeInfo = serviceType.GetTypeInfo();
            if (!typeInfo.IsAbstract) return CreateInstance(serviceType);

            // need it:
            var type = _fallbackAssemblies.SelectMany(s => s.GetTypes()).FirstOrDefault(i => serviceType.IsAssignableFrom(i) && !i.GetTypeInfo().IsInterface);
            if (type != null)
                return AutoResolve(type);

            var fallback = _fallbackProvider?.GetService(serviceType);
            if (fallback != null)
                return fallback;

            if (ThrowIfCantResolve)
                throw new InvalidOperationException($"No registration for {serviceType}");
            return null;
        }

        private object CreateInstance(Type implementationType)
        {
            // type->constructor
            if (!_constructors.TryGetValue(implementationType, out var ctor))
                _constructors[implementationType] = ctor = GetSuitableConstructor(implementationType);

            // constructor->parameters
            if (!_constructorParameters.TryGetValue(ctor, out var parameters))
                _constructorParameters[ctor] = parameters = ctor.GetParameters();

            // activator 
            if (!_activators.TryGetValue(implementationType, out var activator))
                _activators[implementationType] = activator = DynamicMethodFactory.Build(implementationType, ctor, parameters);

            var args = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
                args[i] = AutoResolve(parameters[i].ParameterType);

            return activator(args);
        }

        private static ConstructorInfo GetSuitableConstructor(Type implementationType)
        {
            // Pick the widest constructor; this way we could have parameter-less constructors or
            // simple constructors for testing, without having to do anything special to get the
            // "real" one, such as attributes or other nonsense

            ConstructorInfo[] ctors = implementationType.GetConstructors();
            ConstructorInfo ctor = ctors.OrderByDescending(c => c.GetParameters().Length).First();
            return ctor;
        }

        #region Object Activation Factories

        // See: https://vagifabilov.wordpress.com/2010/04/02/dont-use-activator-createinstance-or-constructorinfo-invoke-use-compiled-lambda-expressions/
        // See: https://rogeralsing.com/2008/02/28/linq-expressions-creating-objects/
        // See: http://stackoverflow.com/questions/2353174/c-sharp-emitting-dynamic-method-delegate-to-load-parametrized-constructor-proble

        public delegate object ObjectActivator(params object[] parameters);

        /// <summary> slowest </summary>
        private static class ActivatorFactory
        {
            public static ObjectActivator Build(Type implementationType)
            {
                return objects => Activator.CreateInstance(implementationType, objects);
            }
        }

        /// <summary> slow </summary>
        private static class ConstructorInvokeFactory
        {
            public static ObjectActivator Build(ConstructorInfo ctor)
            {
                return ctor.Invoke;
            }
        }

        /// <summary> faster </summary>
        private static class CompiledExpressionFactory
        {
            public static ObjectActivator Build(ConstructorInfo ctor)
            {
                ParameterInfo[] paramsInfo = ctor.GetParameters();
                ParameterExpression param = Expression.Parameter(typeof(object[]), "args");
                Expression[] argsExp = new Expression[paramsInfo.Length];
                for (int i = 0; i < paramsInfo.Length; i++)
                {
                    Expression index = Expression.Constant(i);
                    Type paramType = paramsInfo[i].ParameterType;
                    Expression paramAccessorExp = Expression.ArrayIndex(param, index);
                    Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);
                    argsExp[i] = paramCastExp;
                }
                NewExpression newExp = Expression.New(ctor, argsExp);
                LambdaExpression lambda = Expression.Lambda(typeof(ObjectActivator), newExp, param);
                ObjectActivator compiled = (ObjectActivator)lambda.Compile();
                return compiled;
            }
        }

        /// <summary> fastest </summary>
        private static class DynamicMethodFactory
        {
            public static ObjectActivator Build(Type implementationType, ConstructorInfo ctor, IReadOnlyList<ParameterInfo> parameters)
            {
                var dynamicMethod = new DynamicMethod($"{implementationType.FullName}.ctor", implementationType, new[] { typeof(object[]) });
                var il = dynamicMethod.GetILGenerator();
                for (int i = 0; i < parameters.Count; i++)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    switch (i)
                    {
                        case 0: il.Emit(OpCodes.Ldc_I4_0); break;
                        case 1: il.Emit(OpCodes.Ldc_I4_1); break;
                        case 2: il.Emit(OpCodes.Ldc_I4_2); break;
                        case 3: il.Emit(OpCodes.Ldc_I4_3); break;
                        case 4: il.Emit(OpCodes.Ldc_I4_4); break;
                        case 5: il.Emit(OpCodes.Ldc_I4_5); break;
                        case 6: il.Emit(OpCodes.Ldc_I4_6); break;
                        case 7: il.Emit(OpCodes.Ldc_I4_7); break;
                        case 8: il.Emit(OpCodes.Ldc_I4_8); break;
                        default: il.Emit(OpCodes.Ldc_I4, i); break;
                    }
                    il.Emit(OpCodes.Ldelem_Ref);
                    Type paramType = parameters[i].ParameterType;
                    il.Emit(paramType.GetTypeInfo().IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, paramType);
                }
                il.Emit(OpCodes.Newobj, ctor);
                il.Emit(OpCodes.Ret);
                return (ObjectActivator)dynamicMethod.CreateDelegate(typeof(ObjectActivator));
            }
        }

        #endregion

        #endregion

        #region Lifetime Management

        private Func<IDependencyResolver, T> WrapLifecycle<T>(Func<IDependencyResolver, T> builder, Lifetime lifetime) where T : class
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
#if SupportsRequestMemoization
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
#if SupportsRequestMemoization
				case Lifetime.Request:
                    registration = RequestMemoize(builder);
                    break;
#endif
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
            ThreadLocal<T> cache = new ThreadLocal<T>(f);

            return () => cache.Value;
        }

#if ASPNETCORE
        private Func<T> RequestMemoize<T>(Func<T> f)
        {
            return () =>
            {
                IHttpContextAccessor accessor = Resolve<IHttpContextAccessor>();
                if (accessor?.HttpContext == null)
                    return f(); // always new

                var cache = accessor.HttpContext.Items;
                var cacheKey = f.ToString();
                object item;
                if (cache.TryGetValue(cacheKey, out item))
                    return (T)item; // got it

                item = f(); // need it
                cache.Add(cacheKey, item);
                return (T)item;
            };
        }
#endif

        private Func<IDependencyResolver, T> ProcessMemoize<T>(Func<IDependencyResolver, T> f)
        {
            var cache = new ConcurrentDictionary<Type, T>();

            return r => cache.GetOrAdd(typeof(T), v => f(this));
        }

        private Func<IDependencyResolver, T> ThreadMemoize<T>(Func<IDependencyResolver, T> f)
        {
            ThreadLocal<T> cache = new ThreadLocal<T>(() => f(this));

            return r => cache.Value;
        }

#if SupportsRequestMemoization
		private Func<IDependencyResolver, T> RequestMemoize<T>(Func<IDependencyResolver, T> f)
        {
            return r =>
            {
                IHttpContextAccessor accessor = r.Resolve<IHttpContextAccessor>();
                if (accessor?.HttpContext == null)
                    return f(this); // always new

                var cache = accessor.HttpContext.Items;
                var cacheKey = f.ToString();
                object item;
                if (cache.TryGetValue(cacheKey, out item))
                    return (T)item; // got it

                item = f(this); // need it
                cache.Add(cacheKey, item);
                return (T)item;
            };
        }
#endif

        #endregion

        public void Dispose()
        {
            // No scopes, so nothing to dispose
        }

        public object GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }
    }

    public enum Lifetime
    {
        AlwaysNew,
        Permanent,
        Thread,
#if SupportsRequestMemoization
		Request
#endif
    }

    public interface IContainer : IDependencyResolver, IDependencyRegistrar { }

    public interface IDependencyRegistrar : IDisposable
    {
        void Register<T>(Func<T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class;
        void Register<T>(string name, Func<T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class;
        void Register<T>(Func<IDependencyResolver, T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class;
        void Register<T>(string name, Func<IDependencyResolver, T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class;
        void Register<T>(T instance);
    }

    public interface IDependencyResolver : IDisposable
    {
        T Resolve<T>() where T : class;
        T Resolve<T>(string name) where T : class;
        object Resolve(Type serviceType);
        object Resolve(Type serviceType, string name);
    }

#if ASPNETCORE
	public sealed class NoContainerControllerActivator : IControllerActivator
    {
        private readonly IContainer _container;
        public NoContainerControllerActivator(IContainer container)
        {
            _container = container;
        }
        public object CreateInstance(ActionContext context, Type controllerType)
        {
            var controller = _container.Resolve(controllerType);
            return controller;
        }

        public object CreateInstance(ControllerContext context)
        {
            var controllerType = context.ActionDescriptor.ControllerTypeInfo.AsType();
            var controller = _container.Resolve(controllerType);
            return controller;
        }

        public void Release(ControllerContext context, object controller)
        {
            // Lifecycle is managed by the container
        }
    }
#endif
}