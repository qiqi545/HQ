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

namespace HQ.Extensions.Metrics.SplitTesting
{
	public struct ExperimentKey : IComparable<ExperimentKey>
	{
		public string Name { get; }

		public ExperimentKey(string name) : this() => Name = name;

		public bool Equals(ExperimentKey other)
		{
			return string.Equals(Name, other.Name);
		}

		public int CompareTo(ExperimentKey other)
		{
			return Name.CompareTo(other.CompareTo(other));
		}

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(null, obj) && obj is ExperimentKey key && Equals(key);
		}

		public override int GetHashCode()
		{
			return Name != null ? Name.GetHashCode() : 0;
		}

		public static bool operator ==(ExperimentKey left, ExperimentKey right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ExperimentKey left, ExperimentKey right)
		{
			return !left.Equals(right);
		}

		private sealed class NameEqualityComparer : IEqualityComparer<ExperimentKey>
		{
			public bool Equals(ExperimentKey x, ExperimentKey y)
			{
				return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
			}

			public int GetHashCode(ExperimentKey obj)
			{
				return obj.Name != null ? obj.Name.GetHashCode() : 0;
			}
		}

		public static IEqualityComparer<ExperimentKey> NameComparer { get; } = new NameEqualityComparer();
	}
}