// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Blowdart.UI.Internal.Execution;
using System;
using System.Collections.Generic;
using System.Reflection;

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
            if (!Lookup.TryGetValue(type, out var map))
                Lookup.Add(type, map = new Dictionary<string, ObjectMethodExecutor>());

            if (!map.TryGetValue(name, out var executor))
            {
                var methodByName = type.GetMethod(name);
                if (methodByName == null)
                    throw new ArgumentException($"No method on type '{type.FullName}' named '{name}'.");
                map.Add(name, executor = ObjectMethodExecutor.Create(methodByName, type.GetTypeInfo()));
            }

            if (!Instances.TryGetValue(type, out var instance))
                Instances.Add(type, instance = Activator.CreateInstance(type));

            return executor.Execute(instance, args);
        }

        public static object ExecuteMethod(this object instanceOfType, string name, params object[] args)
        {
            var type = instanceOfType.GetType();

            if (!Lookup.TryGetValue(type, out var map))
                Lookup.Add(type, map = new Dictionary<string, ObjectMethodExecutor>());

            if (!map.TryGetValue(name, out var executor))
            {
                var methodByName = type.GetMethod(name);
                if (methodByName == null)
                    throw new ArgumentException($"No method on type '{type.FullName}' named '{name}'.");
                map.Add(name, executor = ObjectMethodExecutor.Create(methodByName, type.GetTypeInfo()));
            }
            
            return executor.Execute(instanceOfType, args);
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
    }
}