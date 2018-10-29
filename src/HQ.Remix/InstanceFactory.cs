// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace HQ.Remix
{
	/// <summary> Provides high-performance object activation. </summary>
	public class InstanceFactory
	{
		public delegate object ObjectActivator(params object[] parameters);

		public delegate object ParameterlessObjectActivator();

		public static InstanceFactory Instance = new InstanceFactory();

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

		/// <summary> Create a typed instance assuming a parameterless constructor. </summary>
		public T CreateInstance<T>()
		{
			return (T) CreateInstance(typeof(T));
		}

		public T CreateInstance<T>(object[] args)
		{
			return (T) CreateInstance(typeof(T), args);
		}

		/// <summary> Create an instance of a type assuming a parameterless constructor. </summary>
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
				_activators[implementationType] = activator = CompiledExpressionFactory.Build(GetOrCacheConstructorForType(implementationType));
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

		private static class CompiledExpressionFactory
		{
			public static ObjectActivator Build(ConstructorInfo ctor)
			{
				var parameters = ctor.GetParameters();
				var parameter = Expression.Parameter(typeof(object[]), "args");
				var arguments = new Expression[parameters.Length];
				for (var i = 0; i < parameters.Length; i++)
				{
					arguments[i] = Expression.Convert(Expression.ArrayIndex(parameter, Expression.Constant(i)),
						parameters[i].ParameterType);
				}
				var lambda = Expression.Lambda(typeof(ObjectActivator), Expression.New(ctor, arguments), parameter);
				var compiled = (ObjectActivator)lambda.Compile();
				return compiled;
			}
		}

		private static class DynamicMethodFactory
		{
			public static ParameterlessObjectActivator Build(Type implementationType, ConstructorInfo ctor)
			{
				var dynamicMethod = new DynamicMethod($"{implementationType.FullName}.0ctor", implementationType, Type.EmptyTypes, true);
				var il = dynamicMethod.GetILGenerator();
				il.Emit(OpCodes.Nop);
				il.Emit(OpCodes.Newobj, ctor);
				il.Emit(OpCodes.Ret);
				return (ParameterlessObjectActivator) dynamicMethod.CreateDelegate(typeof(ParameterlessObjectActivator));
			}
		}
	}
}