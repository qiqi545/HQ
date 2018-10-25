// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;

namespace HQ.Harmony
{
	public struct NameAndType
	{
		public readonly Type type;
		public readonly string name;

		public NameAndType(string name, Type type)
		{
			this.name = name;
			this.type = type;
		}

		public bool Equals(NameAndType other)
		{
			return type == other.type && string.Equals(name, other.name);
		}

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(null, obj) && obj is NameAndType andType && Equals(andType);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((type?.GetHashCode() ?? 0) * 397) ^ (name?.GetHashCode() ?? 0);
			}
		}

		private sealed class TypeNameEqualityComparer : IEqualityComparer<NameAndType>
		{
			public bool Equals(NameAndType x, NameAndType y)
			{
				return x.type == y.type && string.Equals(x.name, y.name);
			}

			public int GetHashCode(NameAndType obj)
			{
				unchecked
				{
					return ((obj.type?.GetHashCode() ?? 0) * 397) ^ (obj.name?.GetHashCode() ?? 0);
				}
			}
		}

		public static IEqualityComparer<NameAndType> TypeNameComparer { get; } = new TypeNameEqualityComparer();
	}
}