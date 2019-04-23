// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Blowdart.UI.Internal.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Primitives;

namespace Blowdart.UI.Internal
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

        public static object ExecuteMethod(this Type type, string name, params object[] args)
        {
            var executor = GetExecutor(type, name, args);

            if (!SameMethodParameters(executor, args))
            {
                throw new InvalidOperationException(
                    $"The intended method was '{name}({string.Join(", ", args.Select(x => x.GetType().Name))})' but " +
                    $"the provided instance's method is '{name}({string.Join(", ", executor.MethodParameters.Select(x => x.ParameterType.Name))})' ");
            }

            if (!Instances.TryGetValue(type, out var instance))
                Instances.Add(type, instance = Pools.ActivatorPool.Create(type));
            return executor.Execute(instance, args);
        }

        public static object ExecuteMethod(this object instanceOfType, string name, params object[] args)
        {
            var instanceType = instanceOfType.GetType();

            var executor = GetExecutor(instanceType, name, args);

            if (!SameMethodParameters(executor, args))
            {
                throw new InvalidOperationException(
                    $"The expected method was '{name}({string.Join(", ", args.Select(x => x.GetType().Name))})' but " +
                    $"the actual method is '{name}({string.Join(", ", executor.MethodParameters.Select(x => x.ParameterType.Name))})' ");
            }
            
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

        public static object ExecuteMethodFunction(this object instanceOfType, string cacheKey, Func<MethodInfo> getMethod, params object[] args)
        {
            var type = instanceOfType.GetType();

            if (!Lookup.TryGetValue(type, out var map))
                Lookup.Add(type, map = new Dictionary<string, ObjectMethodExecutor>());
            
            if (!map.TryGetValue(cacheKey, out var executor))
                map.Add(cacheKey, executor = ObjectMethodExecutor.Create(getMethod(), type.GetTypeInfo()));

            return executor.Execute(instanceOfType, args);
        }

        public static bool IsAnonymous(this Type type)
        {
            return type.Namespace == null && Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute));
        }

        public static string GetPreferredTypeName(this Type type)
        {
	        string typeName;

			//
			// Aliases:
	        if (type == typeof(string))
		        typeName = "string";
	        else if (type == typeof(byte))
		        typeName = "byte";
	        else if (type == typeof(byte?))
		        typeName = "byte?";
			else if (type == typeof(bool))
		        typeName = "bool";
	        else if (type == typeof(bool?))
		        typeName = "bool?";
	        else if (type == typeof(short))
		        typeName = "short";
	        else if (type == typeof(short?))
		        typeName = "short?";
	        else if (type == typeof(ushort))
		        typeName = "ushort";
	        else if (type == typeof(ushort?))
		        typeName = "ushort?";
			else if (type == typeof(int))
		        typeName = "int";
	        else if (type == typeof(int?))
		        typeName = "int?";
	        else if (type == typeof(uint))
		        typeName = "uint";
	        else if (type == typeof(uint?))
		        typeName = "uint?";
	        else if (type == typeof(long))
		        typeName = "long";
	        else if (type == typeof(long?))
		        typeName = "long?";
	        else if (type == typeof(ulong))
		        typeName = "ulong";
	        else if (type == typeof(ulong?))
		        typeName = "ulong?";
	        else if (type == typeof(float))
		        typeName = "float";
			else if (type == typeof(float?))
		        typeName = "float?";
	        else if (type == typeof(double))
		        typeName = "double";
	        else if (type == typeof(double?))
		        typeName = "double?";
	        else if (type == typeof(decimal))
		        typeName = "decimal";
	        else if (type == typeof(decimal?))
		        typeName = "decimal?";
			
			//
			// Value Types:
			else if (type.IsValueType())
		        typeName = type.Name;
	        else if (type.IsNullableValueType())
		        typeName = $"{type.Name}?";
	        else
		        typeName = type.Name;

	        return typeName;
        }

		public static bool IsValueTypeOrNullableValueType(this Type type)
        {
			return type.IsPrimitiveOrNullablePrimitive() || 
			       type == typeof(StringValues) ||
			       type == typeof(StringValues?) ||
				   type == typeof(DateTime) ||
			       type == typeof(DateTime?) ||
			       type == typeof(DateTimeOffset) ||
			       type == typeof(DateTimeOffset?) ||
			       type == typeof(TimeSpan) ||
			       type == typeof(TimeSpan?) ||
			       type == typeof(Guid) ||
			       type == typeof(Guid?);
		}

        public static bool IsValueType(this Type type)
        {
	        return type.IsPrimitive() ||
	               type == typeof(StringValues) ||
	               type == typeof(DateTime) ||
	               type == typeof(DateTimeOffset) ||
	               type == typeof(TimeSpan) ||
	               type == typeof(Guid);
        }

        public static bool IsNullableValueType(this Type type)
        {
	        return type.IsNullablePrimitive() ||
	               type == typeof(StringValues?) ||
	               type == typeof(DateTime?) ||
	               type == typeof(DateTimeOffset?) ||
	               type == typeof(TimeSpan?) ||
	               type == typeof(Guid?);
        }

		public static bool IsPrimitiveOrNullablePrimitive(this Type type)
        {
	        return type.IsPrimitive() || type.IsNullablePrimitive();
        }

        public static bool IsPrimitive(this Type type)
        {
	        return type == typeof(string) ||
	               type == typeof(byte) ||
	               type == typeof(bool) ||
	               type == typeof(short) ||
	               type == typeof(int) ||
	               type == typeof(long) ||
	               type == typeof(float) ||
	               type == typeof(double) ||
	               type == typeof(decimal);
        }

		public static bool IsNullablePrimitive(this Type type)
        {
	        return type == typeof(byte?) ||
	               type == typeof(bool?) ||
	               type == typeof(short?) ||
	               type == typeof(int?) ||
	               type == typeof(long?) ||
	               type == typeof(float?) ||
	               type == typeof(double?) ||
	               type == typeof(decimal?);
        }
	}
}