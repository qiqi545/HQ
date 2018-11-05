// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace HQ.Common.Models
{
	public class TypeRegistry : ITypeRegistry
	{
		private readonly ConcurrentDictionary<string, Type> _typesByName = new ConcurrentDictionary<string, Type>();

		public bool Register(Type type)
		{
			Debug.Assert(type.AssemblyQualifiedName != null, "type.AssemblyQualifiedName != null");
			if (_typesByName.TryGetValue(type.AssemblyQualifiedName, out _))
				throw new ArgumentException("type is already registered");
			_typesByName.AddOrUpdate(type.AssemblyQualifiedName, s => type, (s, t) => t);
			return true;
		}

		public bool RegisterIfNotRegistered(Type type)
		{
			Debug.Assert(type.AssemblyQualifiedName != null, "type.AssemblyQualifiedName != null");
			if (_typesByName.TryGetValue(type.AssemblyQualifiedName, out _))
				return false;
			_typesByName.AddOrUpdate(type.AssemblyQualifiedName, s => type, (s, t) => t);
			return true;
		}

		public bool TryGetType(string name, out Type type)
		{
			// exact match
			if (_typesByName.TryGetValue(name, out type))
				return true;

			// context-free name match
			type = _typesByName.FirstOrDefault(x => x.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				.Value;
			return type != null;
		}
	}
}