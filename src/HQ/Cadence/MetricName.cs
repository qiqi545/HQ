// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Cadence
{
	/// <summary>
	///     A hash key for storing metrics associated by the parent class and name pair
	/// </summary>
	public struct MetricName
	{
		public Type Class { get; }

		public string Name { get; }

		public MetricName(Type @class, string name) : this()
		{
			Class = @class;
			Name = name;
		}

		public bool Equals(MetricName other)
		{
			return Equals(other.Name, Name) && other.Class == Class;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is MetricName name && Equals(name);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Class != null ? Class.GetHashCode() : 0);
			}
		}

		public static bool operator ==(MetricName left, MetricName right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(MetricName left, MetricName right)
		{
			return !left.Equals(right);
		}
	}
}