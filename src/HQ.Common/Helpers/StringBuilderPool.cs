// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;

namespace HQ.Common.Helpers
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