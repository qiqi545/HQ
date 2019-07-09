﻿#region LICENSE

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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HQ.UI.Internal.Execution;
using TypeKitchen;

namespace HQ.UI.Internal
{
	internal static class TypeExtensions
	{
		private static readonly Dictionary<Type, Dictionary<string, ObjectMethodExecutor>> Lookup;
		private static readonly Dictionary<Type, object> Instances;

		static TypeExtensions()
		{
			Lookup = new Dictionary<Type, Dictionary<string, ObjectMethodExecutor>>();
			Instances = new Dictionary<Type, object>();
		}

		public static object ExecuteMethod(this Type type, IServiceProvider serviceProvider, string name,
			params object[] args)
		{
			if (!Instances.TryGetValue(type, out var instance))
				Instances.Add(type, instance = Instancing.CreateInstance(type, serviceProvider));

			return instance.ExecuteMethod(name, args);
		}

		public static object ExecuteMethod(this object instanceOfType, string name, params object[] args)
		{
			var instanceType = instanceOfType.GetType();

			var executor = GetExecutor(instanceType, name, args);

			if (!SameMethodParameters(executor, args))
				throw new InvalidOperationException(
					$"The expected method was '{name}({string.Join(", ", args.Select(x => x.GetType().Name))})' but " +
					$"the actual method is '{name}({string.Join(", ", executor.MethodParameters.Select(x => x.ParameterType.Name))})' ");

			return executor.Execute(instanceOfType, args);
		}

		public static ObjectMethodExecutor GetExecutor(this Type type, string name, params object[] args)
		{
			if (!Lookup.TryGetValue(type, out var map))
				Lookup.Add(type, map = new Dictionary<string, ObjectMethodExecutor>());

			if (!map.TryGetValue(name, out var executor))
			{
				var methodByName = type.GetMethod(name);
				if (methodByName == null)
					throw new ArgumentException($"No method on type '{type.FullName}' named '{name}'");
				map.Add(name, executor = ObjectMethodExecutor.Create(methodByName, type.GetTypeInfo()));
			}

			return executor;
		}

		public static bool SameMethodParameters(this ObjectMethodExecutor executor, object[] args)
		{
			if (executor.MethodParameters.Length != args.Length)
				return false;

			for (var i = 0; i < args.Length; i++)
			{
				var arg = args[i];
				if (arg == null)
					continue;

				var expected = arg.GetType();
				var actual = executor.MethodParameters[i].ParameterType;
				if (actual.IsInterface && actual.IsAssignableFrom(expected))
					continue; // we have an implementation of the interface

				if (expected != actual)
					return false;
			}

			return true;
		}

		public static object ExecuteMethodFunction(this object instanceOfType, string cacheKey,
			Func<MethodInfo> getMethod, params object[] args)
		{
			var type = instanceOfType.GetType();

			if (!Lookup.TryGetValue(type, out var map))
				Lookup.Add(type, map = new Dictionary<string, ObjectMethodExecutor>());

			if (!map.TryGetValue(cacheKey, out var executor))
				map.Add(cacheKey, executor = ObjectMethodExecutor.Create(getMethod(), type.GetTypeInfo()));

			return executor.Execute(instanceOfType, args);
		}


		public static bool ImplementsGeneric(this Type type, Type generic)
		{
			if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == generic)
				return true;

			if (type.GetTypeInfo().ImplementedInterfaces.Any(@interface =>
				@interface.IsConstructedGenericType && @interface.GetGenericTypeDefinition() == generic))
				return true;

			return type.BaseType?.ImplementsGeneric(generic) ?? false;
		}
	}
}