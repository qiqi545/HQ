// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// Changes: Fixes issue where TypeConverter is not consulted when creating an instance, which makes polymorphism impossible in configuration.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace HQ.Extensions.Options
{
	/// <summary>
	/// Static helper class that allows binding strongly typed objects to configuration values.
	/// </summary>
	public static class FastConfigurationBinder
	{
		/// <summary>
		/// Attempts to bind the configuration instance to a new instance of type T.
		/// If this configuration section has a value, that will be used.
		/// Otherwise binding by matching property names against configuration keys recursively.
		/// </summary>
		/// <typeparam name="T">The type of the new instance to bind.</typeparam>
		/// <param name="configuration">The configuration instance to bind.</param>
		/// <returns>The new instance of T if successful, default(T) otherwise.</returns>
		public static T Get<T>(this IConfiguration configuration)
			=> configuration.Get<T>(_ => { });

		/// <summary>
		/// Attempts to bind the configuration instance to a new instance of type T.
		/// If this configuration section has a value, that will be used.
		/// Otherwise binding by matching property names against configuration keys recursively.
		/// </summary>
		/// <typeparam name="T">The type of the new instance to bind.</typeparam>
		/// <param name="configuration">The configuration instance to bind.</param>
		/// <param name="configureOptions">Configures the binder options.</param>
		/// <returns>The new instance of T if successful, default(T) otherwise.</returns>
		public static T Get<T>(this IConfiguration configuration, Action<BinderOptions> configureOptions)
		{
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));
			var result = configuration.Get(typeof(T), configureOptions);
			return result == null ? default : (T) result;
		}

		/// <summary>
		/// Attempts to bind the configuration instance to a new instance of type T.
		/// If this configuration section has a value, that will be used.
		/// Otherwise binding by matching property names against configuration keys recursively.
		/// </summary>
		/// <param name="configuration">The configuration instance to bind.</param>
		/// <param name="type">The type of the new instance to bind.</param>
		/// <returns>The new instance if successful, null otherwise.</returns>
		public static object Get(this IConfiguration configuration, Type type)
			=> configuration.Get(type, _ => { });

		/// <summary>
		/// Attempts to bind the configuration instance to a new instance of type T.
		/// If this configuration section has a value, that will be used.
		/// Otherwise binding by matching property names against configuration keys recursively.
		/// </summary>
		/// <param name="configuration">The configuration instance to bind.</param>
		/// <param name="type">The type of the new instance to bind.</param>
		/// <param name="configureOptions">Configures the binder options.</param>
		/// <returns>The new instance if successful, null otherwise.</returns>
		public static object Get(this IConfiguration configuration, Type type, Action<BinderOptions> configureOptions)
		{
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			var options = new BinderOptions();
			configureOptions?.Invoke(options);
			return BindInstance(type,  null,  configuration, options);
		}

		/// <summary>
		/// Attempts to bind the given object instance to the configuration section specified by the key by matching property names against configuration keys recursively.
		/// </summary>
		/// <param name="configuration">The configuration instance to bind.</param>
		/// <param name="key">The key of the configuration section to bind.</param>
		/// <param name="instance">The object to bind.</param>
		public static void FastBind(this IConfiguration configuration, string key, object instance)
			=> configuration.GetSection(key).FastBind(instance);

		/// <summary>
		/// Attempts to bind the given object instance to configuration values by matching property names against configuration keys recursively.
		/// </summary>
		/// <param name="configuration">The configuration instance to bind.</param>
		/// <param name="instance">The object to bind.</param>
		public static void FastBind(this IConfiguration configuration, object instance)
			=> configuration.FastBind(instance, o => { });

		/// <summary>
		/// Attempts to bind the given object instance to configuration values by matching property names against configuration keys recursively.
		/// </summary>
		/// <param name="configuration">The configuration instance to bind.</param>
		/// <param name="instance">The object to bind.</param>
		/// <param name="configureOptions">Configures the binder options.</param>
		public static void FastBind(this IConfiguration configuration, object instance, Action<BinderOptions> configureOptions)
		{
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			if (instance == null)
				return;

			var options = new BinderOptions();
			configureOptions?.Invoke(options);
			BindInstance(instance.GetType(), instance, configuration, options);
		}

		/// <summary>
		/// Extracts the value with the specified key and converts it to type T.
		/// </summary>
		/// <typeparam name="T">The type to convert the value to.</typeparam>
		/// <param name="configuration">The configuration.</param>
		/// <param name="key">The key of the configuration section's value to convert.</param>
		/// <returns>The converted value.</returns>
		public static T GetValue<T>(this IConfiguration configuration, string key)
		{
			return GetValue(configuration, key, default(T));
		}

		/// <summary>
		/// Extracts the value with the specified key and converts it to type T.
		/// </summary>
		/// <typeparam name="T">The type to convert the value to.</typeparam>
		/// <param name="configuration">The configuration.</param>
		/// <param name="key">The key of the configuration section's value to convert.</param>
		/// <param name="defaultValue">The default value to use if no value is found.</param>
		/// <returns>The converted value.</returns>
		public static T GetValue<T>(this IConfiguration configuration, string key, T defaultValue)
		{
			return (T) GetValue(configuration, typeof(T), key, defaultValue);
		}

		/// <summary>
		/// Extracts the value with the specified key and converts it to the specified type.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		/// <param name="type">The type to convert the value to.</param>
		/// <param name="key">The key of the configuration section's value to convert.</param>
		/// <returns>The converted value.</returns>
		public static object GetValue(this IConfiguration configuration, Type type, string key)
		{
			return GetValue(configuration, type, key, defaultValue: null);
		}

		/// <summary>
		/// Extracts the value with the specified key and converts it to the specified type.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		/// <param name="type">The type to convert the value to.</param>
		/// <param name="key">The key of the configuration section's value to convert.</param>
		/// <param name="defaultValue">The default value to use if no value is found.</param>
		/// <returns>The converted value.</returns>
		public static object GetValue(this IConfiguration configuration, Type type, string key, object defaultValue)
		{
			var value = configuration.GetSection(key).Value;
			return value != null ? ConvertValue(type, value) : defaultValue;
		}

		private static void BindNonScalar(this IConfiguration configuration, object instance, BinderOptions options)
		{
			if (instance == null)
				return;

			foreach (var property in GetAllProperties(instance.GetType().GetTypeInfo()))
			{
				// We don't support set only, non-public, or indexer properties
				if (property.GetMethod == null || (!options.BindNonPublicProperties && !property.GetMethod.IsPublic) ||
				    property.GetMethod.GetParameters().Length > 0)
				{
					return;
				}

				var propertyValue = property.GetValue(instance);
				var hasSetter = property.SetMethod != null && (property.SetMethod.IsPublic || options.BindNonPublicProperties);

				if (propertyValue == null && !hasSetter)
				{
					// Property doesn't have a value and we cannot set it so there is no
					// point in going further down the graph
					return;
				}

				propertyValue = BindInstance(property.PropertyType, propertyValue, configuration.GetSection(property.Name), options);

				if (propertyValue != null && hasSetter)
				{
					property.SetValue(instance, propertyValue);
				}
			}
		}
        
		private static object BindToCollection(Type typeInfo, IConfiguration config, BinderOptions options)
		{
			var type = typeof(List<>).MakeGenericType(typeInfo.GenericTypeArguments[0]);
			var instance = CreateInstance(ref type);
			BindCollection(instance, type, config, options);
			return instance;
		}

		// Try to create an array/dictionary instance to back various collection interfaces
		private static object AttemptBindToCollectionInterfaces(Type type, IConfiguration config, BinderOptions options)
		{
			var typeInfo = type.GetTypeInfo();

			if (!typeInfo.IsInterface)
				return null;

			var collectionInterface = FindOpenGenericInterface(typeof(IReadOnlyList<>), type);
			if (collectionInterface != null)
				return BindToCollection(typeInfo, config, options);

			collectionInterface = FindOpenGenericInterface(typeof(IReadOnlyDictionary<,>), type);
			if (collectionInterface != null)
			{
				var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeInfo.GenericTypeArguments[0], typeInfo.GenericTypeArguments[1]);
				var instance = CreateInstance(ref dictionaryType);
				BindDictionary(instance, dictionaryType, config, options);
				return instance;
			}

			collectionInterface = FindOpenGenericInterface(typeof(IDictionary<,>), type);
			if (collectionInterface != null)
			{
				var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeInfo.GenericTypeArguments[0], typeInfo.GenericTypeArguments[1]);
				var instance = CreateInstance(ref dictionaryType);
				BindDictionary(instance, collectionInterface, config, options);
				return instance;
			}

			collectionInterface = FindOpenGenericInterface(typeof(IReadOnlyCollection<>), type);
			if (collectionInterface != null)
				return BindToCollection(typeInfo, config, options);

			collectionInterface = FindOpenGenericInterface(typeof(ICollection<>), type);
			if (collectionInterface != null)
				return BindToCollection(typeInfo, config, options);

			collectionInterface = FindOpenGenericInterface(typeof(IEnumerable<>), type);
			return collectionInterface != null ? BindToCollection(typeInfo, config, options) : null;
		}

		private static object BindInstance(Type type, object instance, IConfiguration config, BinderOptions options)
		{
			// if binding IConfigurationSection, break early
			if (type == typeof(IConfigurationSection))
				return config;

			var section = config as IConfigurationSection;
			var configValue = section?.Value;
			if (configValue != null && TryConvertValue(type, configValue, out var convertedValue, out var error))
			{
				if (error != null)
					throw error;

				// Leaf nodes are always reinitialized
				return convertedValue;
			}

			if (config == null || !config.GetChildren().Any())
				return instance;

			// If we don't have an instance, try to create one
			if (instance == null)
			{
				// We are already done if binding to a new collection instance worked
				instance = AttemptBindToCollectionInterfaces(type, config, options);
				if (instance != null)
				{
					return instance;
				}

				instance = CreateInstance(ref type);
			}

			// See if its a Dictionary
			var collectionInterface = FindOpenGenericInterface(typeof(IDictionary<,>), type);
			if (collectionInterface != null)
			{
				BindDictionary(instance, collectionInterface, config, options);
			}
			else if (type.IsArray)
			{
				instance = BindArray((Array) instance, config, options);
			}
			else
			{
				// See if its an ICollection
				collectionInterface = FindOpenGenericInterface(typeof(ICollection<>), type);
				if (collectionInterface != null)
				{
					BindCollection(instance, collectionInterface, config, options);
				}
				// Something else
				else
				{
					BindNonScalar(config, instance, options);
				}
			}

			return instance;
		}

		private static object CreateInstance(ref Type type)
		{
			object instance;

			var typeInfo = type.GetTypeInfo();

			if (typeInfo.IsInterface || typeInfo.IsAbstract)
				throw new InvalidOperationException();

			if (type.IsArray)
			{
				if (typeInfo.GetArrayRank() > 1)
					throw new InvalidOperationException();

				instance = Array.CreateInstance(typeInfo.GetElementType() ?? throw new InvalidOperationException(), 0);
			}
			else
			{
				var hasDefaultConstructor = typeInfo.DeclaredConstructors.Any(ctor => ctor.IsPublic && ctor.GetParameters().Length == 0);
				if (!hasDefaultConstructor)
					throw new InvalidOperationException();

				try
				{
					instance = Activator.CreateInstance(type);
				}
				catch (Exception)
				{
					throw new InvalidOperationException();
				}
			}

			var converter = TypeDescriptor.GetConverter(type);
			if (!converter.CanConvertFrom(type))
				return instance;

			// Change: give TypeConverter a chance to change this type
			instance = converter.ConvertFrom(instance);
			if (instance != null)
				type = instance.GetType();

			return instance;
		}

		private static void BindDictionary(object dictionary, Type dictionaryType, IConfiguration config, BinderOptions options)
		{
			var typeInfo = dictionaryType.GetTypeInfo();

			// IDictionary<K,V> is guaranteed to have exactly two parameters
			var keyType = typeInfo.GenericTypeArguments[0];
			var valueType = typeInfo.GenericTypeArguments[1];
			var keyTypeIsEnum = keyType.GetTypeInfo().IsEnum;

			if (keyType != typeof(string) && !keyTypeIsEnum)
			{
				// We only support string and enum keys
				return;
			}

			var setter = typeInfo.GetDeclaredProperty("Item");
			foreach (var child in config.GetChildren())
			{
				var item = BindInstance(valueType, null, child, options);
				if (item == null)
					continue;

				if (keyType == typeof(string))
				{
					var key = child.Key;
					setter.SetValue(dictionary, item, new object[] { key });
				}
				else if (keyTypeIsEnum)
				{
					var key = Convert.ToInt32(Enum.Parse(keyType, child.Key));
					setter.SetValue(dictionary, item, new object[] { key });
				}
			}
		}

		private static void BindCollection(object collection, Type collectionType, IConfiguration config, BinderOptions options)
		{
			var typeInfo = collectionType.GetTypeInfo();

			// ICollection<T> is guaranteed to have exactly one parameter
			var itemType = typeInfo.GenericTypeArguments[0];
			var addMethod = typeInfo.GetDeclaredMethod("Add");

			foreach (var section in config.GetChildren())
			{
				try
				{
					var item = BindInstance(itemType, null, section, options);
					if (item != null)
						addMethod.Invoke(collection, new[] { item });
				}
				catch
				{
					// ignored
				}
			}
		}

		private static Array BindArray(Array source, IConfiguration config, BinderOptions options)
		{
			var children = config.GetChildren().ToArray();
			var arrayLength = source.Length;
			var elementType = source.GetType().GetElementType();
			var newArray = Array.CreateInstance(elementType ?? throw new InvalidOperationException(), arrayLength + children.Length);

			// binding to array has to preserve already initialized arrays with values
			if (arrayLength > 0)
			{
				Array.Copy(source, newArray, arrayLength);
			}

			for (var i = 0; i < children.Length; i++)
			{
				try
				{
					var item = BindInstance(elementType, null, children[i], options);
					if (item != null)
						newArray.SetValue(item, arrayLength + i);
				}
				catch
				{
					// ignored
				}
			}

			return newArray;
		}

		private static bool TryConvertValue(Type type, string value, out object result, out Exception error)
		{
			while (true)
			{
				error = null;
				result = null;
				if (type == typeof(object))
				{
					result = value;
					return true;
				}

				if (type.GetTypeInfo().IsGenericType && type?.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					if (string.IsNullOrEmpty(value))
					{
						return true;
					}

					type = Nullable.GetUnderlyingType(type);
					continue;
				}

				var converter = TypeDescriptor.GetConverter(type ?? throw new ArgumentNullException(nameof(type)));
                if (!converter.CanConvertFrom(typeof(string)))
					return false;

				try
				{
					result = converter.ConvertFromInvariantString(value);
				}
				catch (Exception)
				{
					error = new InvalidOperationException();
				}

				return true;
			}
		}

		private static object ConvertValue(Type type, string value)
		{
			TryConvertValue(type, value, out var result, out var error);
			if (error != null)
			{
				throw error;
			}
			return result;
		}

		private static Type FindOpenGenericInterface(Type expected, Type actual)
		{
			var actualTypeInfo = actual.GetTypeInfo();
			if (actualTypeInfo.IsGenericType && actual.GetGenericTypeDefinition() == expected)
				return actual;

			var interfaces = actualTypeInfo.ImplementedInterfaces;
			foreach (var interfaceType in interfaces)
			{
				if (interfaceType.GetTypeInfo().IsGenericType && interfaceType.GetGenericTypeDefinition() == expected)
					return interfaceType;
			}

			return null;
		}

		private static IEnumerable<PropertyInfo> GetAllProperties(TypeInfo type)
		{
			var allProperties = new List<PropertyInfo>();

			do
			{
				allProperties.AddRange(type.DeclaredProperties);
				type = type.BaseType.GetTypeInfo();
			}
			while (type != typeof(object).GetTypeInfo());

			return allProperties;
		}
	}
}