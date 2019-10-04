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
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using HQ.Extensions.CodeGeneration.Internal;

namespace HQ.Extensions.CodeGeneration
{
	/// <summary> Provides high-performance object activation. </summary>
	public class InstanceFactory
	{
		public delegate object ObjectActivator(params object[] parameters);

		public delegate object ParameterlessObjectActivator();

		public static InstanceFactory Default = new InstanceFactory();

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
		public object CreateInstance(Type type)
		{
			Contract.Assert(!type.IsAbstract && !type.IsInterface,
				"Cannot create instances of static, abstract, or interface types.");

			// activator 
			if (_emptyActivators.TryGetValue(type, out var activator))
				return activator();
			var ctor = type.GetConstructor(Type.EmptyTypes);
			_emptyActivators[type] = activator = DynamicMethodFactory.Build(type, ctor);
			return activator();
		}

		/// <summary> Create an instance of a type assuming a set of parameters. </summary>
		public object CreateInstance(Type type, object[] args)
		{
			if (args == null || args.Length == 0)
				return CreateInstance(type);

			// activator 
			if (!_activators.TryGetValue(type, out var activator))
				_activators[type] = activator = CompiledExpressionFactory.Build(GetOrCacheConstructorForType(type));

			return activator(args);
		}

		public ConstructorInfo GetOrCacheConstructorForType(Type type)
		{
			// type->constructor
			if (!_constructors.TryGetValue(type, out var ctor))
				_constructors[type] = ctor = type.GetWidestConstructor();
			return ctor;
		}

		public ParameterInfo[] GetOrCacheParametersForConstructor(ConstructorInfo ctor)
		{
			// constructor->parameters
			if (!_constructorParameters.TryGetValue(ctor, out var parameters))
				_constructorParameters[ctor] = parameters = ctor.GetParameters();
			return parameters;
		}

		internal static class CompiledExpressionFactory
		{
			public static ObjectActivator Build(ConstructorInfo ctor)
			{
				var parameters = ctor.GetParameters();
				var parameter = Expression.Parameter(typeof(object[]), "args");
				var arguments = new Expression[parameters.Length];
				for (var i = 0; i < parameters.Length; i++)
					arguments[i] = Expression.Convert(Expression.ArrayIndex(parameter, Expression.Constant(i)),
						parameters[i].ParameterType);
				var lambda = Expression.Lambda(typeof(ObjectActivator), Expression.New(ctor, arguments), parameter);
				var compiled = (ObjectActivator) lambda.Compile();
				return compiled;
			}
		}

		internal static class DynamicMethodFactory
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
		}
	}
}