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
using System.Collections.Generic;
using HQ.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using TypeKitchen;
using TypeKitchen.Differencing;

namespace HQ.Platform.Operations.Models
{
	public class ConfigurationService
	{
		private readonly IConfigurationRoot _root;
		private readonly IServiceProvider _serviceProvider;
		private readonly IEnumerable<ICustomConfigurationBinder> _customBinders;

		private readonly Dictionary<CacheKey, object> _boundInstances;
		private readonly Dictionary<CacheKey, IChangeToken> _changeTokens;

		public ConfigurationService(IConfigurationRoot root, IServiceProvider serviceProvider, IEnumerable<ICustomConfigurationBinder> customBinders)
		{
			_root = root;
			_serviceProvider = serviceProvider;
			_customBinders = customBinders;
			_boundInstances = new Dictionary<CacheKey, object>();
			_changeTokens = new Dictionary<CacheKey, IChangeToken>();
		}

		public object Get(Type type, string section)
		{
			var config = _root.GetSection(section?.Replace("/", ":"));
			if (config == null)
				return null;

			var key = new CacheKey(type.FullName, section);

			MaybeRegisterChangeCallback(type, key);

			if (!_boundInstances.TryGetValue(key, out var instance))
			{
				instance = BindTemplateInstance(type, config);

				if(instance != null)
					_boundInstances.Add(key, instance);
			}

			return instance;
		}

		private struct CacheKey : IEquatable<CacheKey>, IComparable<CacheKey>, IComparable
		{
			private readonly string _type;
			private readonly string _section;

			public CacheKey(string type, string section)
			{
				_type = type;
				_section = section;
			}

			public int CompareTo(CacheKey other)
			{
				var typeComparison = string.Compare(_type, other._type, StringComparison.Ordinal);
				return typeComparison != 0 ? typeComparison : string.Compare(_section, other._section, StringComparison.Ordinal);
			}

			public int CompareTo(object obj)
			{
				if (ReferenceEquals(null, obj))
					return 1;
				return obj is CacheKey other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(CacheKey)}");
			}

			public static bool operator <(CacheKey left, CacheKey right)
			{
				return left.CompareTo(right) < 0;
			}

			public static bool operator >(CacheKey left, CacheKey right)
			{
				return left.CompareTo(right) > 0;
			}

			public static bool operator <=(CacheKey left, CacheKey right)
			{
				return left.CompareTo(right) <= 0;
			}

			public static bool operator >=(CacheKey left, CacheKey right)
			{
				return left.CompareTo(right) >= 0;
			}

			public bool Equals(CacheKey other)
			{
				return _type == other._type && _section == other._section;
			}

			public override bool Equals(object obj)
			{
				return obj is CacheKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return ((_type != null ? _type.GetHashCode() : 0) * 397) ^ (_section != null ? _section.GetHashCode() : 0);
				}
			}

			public static bool operator ==(CacheKey left, CacheKey right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(CacheKey left, CacheKey right)
			{
				return !left.Equals(right);
			}
		}

		private void Callback(object state)
		{
			var key = (CacheKey) state;
			_boundInstances.Remove(key);
			_changeTokens.Remove(key);
		}
		
		private void MaybeRegisterChangeCallback(Type type, CacheKey key)
		{
			if (_changeTokens.ContainsKey(key))
				return;

			var sourceType = typeof(IOptionsChangeTokenSource<>).MakeGenericType(type);
			var source = _serviceProvider.GetService(sourceType);
			if (source != null)
			{
				var method = sourceType.GetMethod(nameof(IOptionsChangeTokenSource<object>.GetChangeToken));
				var changeToken = method?.Invoke(source, new object[] { }) as IChangeToken;
				changeToken?.RegisterChangeCallback(Callback, key);
				_changeTokens.Add(key, changeToken);
			}
			else
				_changeTokens.Add(key, default);
		}

		private static readonly object Sync = new object();
		private readonly IDictionary<Type, ulong> _empties = new Dictionary<Type, ulong>();

		private object BindTemplateInstance(Type type, IConfiguration config)
		{
			lock (Sync)
			{
				var template = Instancing.CreateInstance(type);
				if(!_empties.TryGetValue(type, out _))
					_empties.Add(type, ValueHash.ComputeHash(template));
				
				config.FastBind(template, _customBinders);
				return ValueHash.ComputeHash(template) == _empties[type] ? null : template;
			}
		}
	}
}