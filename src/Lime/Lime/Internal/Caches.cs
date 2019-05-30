// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TypeKitchen;

namespace Lime.Internal
{
	internal static class Caches
	{
		public static class Introspection
		{
			private static Dictionary<string, MethodInfo> _handlerNames;

			private static Dictionary<string, MethodInfo> _handlers;

			private static ImmutableHashSet<MethodInfo> _methods;

			private static Dictionary<MethodInfo, NameValueCollection> _meta;

			private static Dictionary<MethodInfo, UiSystem> _systems;

			public static void Clear()
			{
				_handlerNames?.Clear();
				_handlers?.Clear();
				_methods?.Clear();
				_meta?.Clear();
				_systems?.Clear();
			}

			public static Dictionary<string, MethodInfo> IntrospectHandlerNames()
			{
				if (_handlerNames != null)
					return _handlerNames;

				var methods = IntrospectMethods();
				var methodsWithAttribute =
					methods.Where(x => Attribute.IsDefined(x, typeof(HandlerNameAttribute))).ToList();
				var kvp = methodsWithAttribute.Select(x =>
				{
					var handlerName =
						(HandlerNameAttribute) Attribute.GetCustomAttribute(x, typeof(HandlerNameAttribute));
					return new KeyValuePair<string, MethodInfo>(handlerName.Name, x);
				}).ToList();

				var duplicates = kvp.ToLookup(x => x.Value, x => x.Key).Where(x => x.Count() > 1);
				foreach (var duplicate in duplicates)
				{
					var values = duplicate.Aggregate(string.Empty, (s, v) => $"{s}, {v}");
					var message = $"Duplicate entries found for handlers with name \"{duplicate.Key}\":{values}.";
					Trace.TraceWarning(message);
				}

				_handlerNames = kvp.Distinct().ToDictionary(k => k.Key, v => v.Value);
				return _handlerNames;
			}

			public static Dictionary<string, MethodInfo> IntrospectHandlers()
			{
				if (_handlers != null)
					return _handlers;

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

				_handlers = kvp.Distinct().ToDictionary(k => k.Key, v => v.Value);
				return _handlers;
			}

			public static ImmutableHashSet<MethodInfo> IntrospectMethods()
			{
				if (_methods != null)
					return _methods;

				var types = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(x => x.GetTypes().OrderByDescending(t => t.IsPublic));
				var methods = types.SelectMany(x => x.GetMethods());
				_methods = methods.ToImmutableHashSet();
				return _methods;
			}

			public static Dictionary<MethodInfo, NameValueCollection> IntrospectMeta()
			{
				if (_meta != null)
					return _meta;

				var methods = IntrospectMethods();

				var result = new Dictionary<MethodInfo, NameValueCollection>();

				// 
				// Parent Class Meta (flatten):
				foreach (var child in methods.Where(x =>
					x.DeclaringType != null && Attribute.IsDefined(x.DeclaringType, typeof(MetaAttribute))))
				{
					var parent = child.DeclaringType;
					if (parent == null)
						continue;

					if (!result.TryGetValue(child, out var collection))
						result.Add(child, collection = new NameValueCollection(StringComparer.OrdinalIgnoreCase));

					var attributes = Attribute.GetCustomAttributes(parent, typeof(MetaAttribute)).Cast<MetaAttribute>();
					foreach (var meta in attributes)
						if (meta.Name.Equals("title", StringComparison.OrdinalIgnoreCase))
							collection[meta.Name] = meta.Content;
						else
							collection.Add(meta.Name, meta.Content);
				}

				//
				// Child Method Meta (merges with parent, but replaces title):
				foreach (var child in methods.Where(x => Attribute.IsDefined(x, typeof(MetaAttribute))))
				{
					if (!result.TryGetValue(child, out var collection))
						result.Add(child, collection = new NameValueCollection(StringComparer.OrdinalIgnoreCase));

					var attributes = Attribute.GetCustomAttributes(child, typeof(MetaAttribute)).Cast<MetaAttribute>();
					foreach (var meta in attributes)
						if (meta.Name.Equals("title", StringComparison.OrdinalIgnoreCase))
							collection[meta.Name] = meta.Content;
						else
							collection.Add(meta.Name, meta.Content);
				}

				_meta = result;
				return _meta;
			}

			public static Dictionary<MethodInfo, UiSystem> IntrospectSystems()
			{
				if (_systems != null)
					return _systems;

				var methods = IntrospectMethods();
				var result = new Dictionary<MethodInfo, UiSystem>();

				//
				// Parent:
				foreach (var child in methods.Where(x =>
					x.DeclaringType != null && Attribute.IsDefined(x.DeclaringType, typeof(UiSystemAttribute))))
				{
					var parent = child.DeclaringType;
					if (parent == null)
						continue;
					var attribute = (UiSystemAttribute) Attribute.GetCustomAttribute(parent, typeof(UiSystemAttribute));
					result[child] = (UiSystem) Instancing.CreateInstance(attribute.Type);
				}

				//
				// Child (overwrites parent):
				foreach (var child in methods.Where(x => Attribute.IsDefined(x, typeof(UiSystemAttribute))))
				{
					var attribute = (UiSystemAttribute) Attribute.GetCustomAttribute(child, typeof(UiSystemAttribute));
					result[child] = (UiSystem) Instancing.CreateInstance(attribute.Type);
				}

				_systems = result;
				return result;
			}
		}
	}
}