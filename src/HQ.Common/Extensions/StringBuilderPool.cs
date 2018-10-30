using System;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;

namespace HQ.Common.Extensions
{
	public static class StringBuilderPool
	{
		public static string Scoped(Action<StringBuilder> closure)
		{
			var sb = PooledStringBuilder.GetInstance();
			closure(sb);
			return sb.ToStringAndFree();
		}

		public static string Scoped(Action<StringBuilder> closure, int startIndex, int length)
		{
			var sb = PooledStringBuilder.GetInstance();
			closure(sb);
			return sb.ToStringAndFree(startIndex, length);
		}
	}
}
