// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Linq;

namespace HQ.Rosetta.Queryable
{
	public interface IQueryableProvider<T>
	{
		/// <summary>
		///     Determines whether to throw a <see cref="NotSupportedException" /> if <see cref="Queryable" /> is accessed.
		///     If this is <code>false</code>, then developers may potentially access a <see cref="Queryable" /> that is
		///     unpredictable, or, if none is available, may materialize the entire underlying collection in order to
		///     perform a query.
		/// </summary>
		bool ThrowOnQueryableAccess { get; }

		IQueryable<T> Queryable { get; }
		ISafeQueryable<T> SafeQueryable { get; }
	}
}