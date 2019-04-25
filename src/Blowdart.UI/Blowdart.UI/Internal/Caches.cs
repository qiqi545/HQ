// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TypeKitchen;

namespace Blowdart.UI.Internal
{
	internal static class Caches
	{
		public static class Introspection
		{
			public static void Clear()
			{
				_methods?.Clear();
				_handlerMethodsWithName?.Clear();
			}

			private static Dictionary<string, MethodInfo> _handlerMethodsWithName;

			public static Dictionary<string, MethodInfo> IntrospectHandlerNames()
			{
				if (_handlerMethodsWithName != null)
					return _handlerMethodsWithName;

				var methods = IntrospectMethods();
				var methodsWithAttribute = methods.Where(x => Attribute.IsDefined(x, typeof(HandlerNameAttribute))).ToList();
				var kvp = methodsWithAttribute.Select(x =>
				{
					var handlerName = (HandlerNameAttribute) Attribute.GetCustomAttribute(x, typeof(HandlerNameAttribute));
					return new KeyValuePair<string, MethodInfo>(handlerName.Name, x);
				}).ToList();

				var duplicates = kvp.ToLookup(x => x.Value, x => x.Key).Where(x => x.Count() > 1);
				foreach (var duplicate in duplicates)
				{
					var values = duplicate.Aggregate(string.Empty, (s, v) => $"{s}, {v}");
					var message = $"Duplicate entries found for handlers with name \"{duplicate.Key}\":{values}.";
					Trace.TraceWarning(message);
				}

				_handlerMethodsWithName = kvp.Distinct().ToDictionary(k => k.Key, v => v.Value);
				return _handlerMethodsWithName;
			}

			public static Dictionary<string, MethodInfo> IntrospectHandlers()
			{
				var methods = IntrospectMethods();
				var kvp = methods.Where(x => Attribute.IsDefined(x, typeof(HandlerAttribute))).Select(x =>
				{
					var handlerName = (HandlerAttribute) Attribute.GetCustomAttribute(x, typeof(HandlerAttribute));
					return new KeyValuePair<string, MethodInfo>(handlerName.Template, x);
				}).ToList();

				var duplicates = kvp.ToLookup(x => x.Value, x => x.Key).Where(x => x.Count() > 1);
				foreach (var duplicate in duplicates)
				{
					var values = duplicate.Aggregate(string.Empty, (s, v) => $"{s}, {v}");
					var message = $"Duplicate entries found for handlers with template \"{duplicate.Key}\":{values}";
					Trace.TraceWarning(message);
				}
				return kvp.Distinct().ToDictionary(k => k.Key, v => v.Value);
			}

			private static ImmutableHashSet<MethodInfo> _methods;
			public static ImmutableHashSet<MethodInfo> IntrospectMethods()
			{
				if (_methods != null)
					return _methods;

				var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes().OrderByDescending(t => t.IsPublic));
				var methods = types.SelectMany(x => x.GetMethods());
				_methods = methods.ToImmutableHashSet();
				return _methods;
			}

			public static Dictionary<MethodInfo, NameValueCollection> IntrospectMeta()
			{
				var methods = IntrospectMethods();

				var result = new Dictionary<MethodInfo, NameValueCollection>();

				// 
				// Parent Class Meta (flatten):
				foreach (var child in methods.Where(x => x.DeclaringType != null && Attribute.IsDefined(x.DeclaringType, typeof(MetaAttribute))))
				{
					var parent = child.DeclaringType;
					if (parent == null)
						continue;

					if (!result.TryGetValue(child, out var collection))
						result.Add(child, collection = new NameValueCollection(StringComparer.OrdinalIgnoreCase));

					var attributes = Attribute.GetCustomAttributes(parent, typeof(MetaAttribute)).Cast<MetaAttribute>();
					foreach (var meta in attributes)
					{
						if (meta.Name.Equals("title", StringComparison.OrdinalIgnoreCase))
							collection[meta.Name] = meta.Content;
						else
							collection.Add(meta.Name, meta.Content);
					}
				}

				//
				// Child Method Meta (merges with parent, but replaces title):
				foreach (var child in methods.Where(x => Attribute.IsDefined(x, typeof(MetaAttribute))))
				{
					if (!result.TryGetValue(child, out var collection))
						result.Add(child, collection = new NameValueCollection(StringComparer.OrdinalIgnoreCase));

					var attributes = Attribute.GetCustomAttributes(child, typeof(MetaAttribute)).Cast<MetaAttribute>();
					foreach (var meta in attributes)
					{
						if (meta.Name.Equals("title", StringComparison.OrdinalIgnoreCase))
							collection[meta.Name] = meta.Content;
						else
							collection.Add(meta.Name, meta.Content);
					}
				}
				
				return result;
			}

			private static Dictionary<MethodInfo, UiSystem> _systems;
			public static Dictionary<MethodInfo, UiSystem> IntrospectSystems()
			{
				if (_systems != null)
					return _systems;

				var methods = IntrospectMethods();
				var result = new Dictionary<MethodInfo, UiSystem>();

				//
				// Parent:
				foreach (var child in methods.Where(x => x.DeclaringType != null && Attribute.IsDefined(x.DeclaringType, typeof(UiSystemAttribute))))
				{
					var parent = child.DeclaringType;
					if (parent == null)
						continue;
					var attribute = (UiSystemAttribute) Attribute.GetCustomAttribute(parent, typeof(UiSystemAttribute));
					result[child] = (UiSystem) ActivatorCache.Create(attribute.Type);
				}

				//
				// Child (overwrites parent):
				foreach (var child in methods.Where(x => Attribute.IsDefined(x, typeof(UiSystemAttribute))))
				{
					var attribute = (UiSystemAttribute) Attribute.GetCustomAttribute(child, typeof(UiSystemAttribute));
					result[child] = (UiSystem) ActivatorCache.Create(attribute.Type);
				}

				_systems = result;
				return result;
			}
		}

		public static class ActivatorCache
		{
			public static readonly Dictionary<Type, CreateInstance> Factory = new Dictionary<Type, CreateInstance>();

			public static T Create<T>() => (T) GetOrBuildActivator<T>()();
			public static object Create(Type type) => GetOrBuildActivator(type)();

			private static CreateInstance GetOrBuildActivator<T>() => GetOrBuildActivator(typeof(T));
			private static CreateInstance GetOrBuildActivator(Type type)
			{
				lock (Factory)
				{
					if (Factory.TryGetValue(type, out var activator))
						return activator;
					lock (Factory)
					{
						if (!Factory.TryGetValue(type, out activator))
							Factory.Add(type, activator = Activation.DynamicMethodWeakTyped(type.GetConstructor(Type.EmptyTypes)));
					}
					return activator;
				}
			}
		}
	}
}