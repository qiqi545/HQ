using System;
using System.Collections.Generic;
using System.Linq;

namespace HQ.Flow
{
	internal static class EnumerableExtensions
	{
		public static IEnumerable<T> NetworkOrder<T>(this IEnumerable<T> enumerator, Func<T, string> getName)
		{
			return enumerator.OrderBy(getName, StringComparer.Ordinal);
		}
	}
}
