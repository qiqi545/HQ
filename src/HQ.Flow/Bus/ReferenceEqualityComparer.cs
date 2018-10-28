// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HQ.Flow.Bus
{
	internal class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
	{
		private ReferenceEqualityComparer()
		{
		}

		public static ReferenceEqualityComparer<T> Instance { get; } = new ReferenceEqualityComparer<T>();

		public bool Equals(T x, T y)
		{
			return ReferenceEquals(x, y);
		}

		public int GetHashCode(T obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}
}