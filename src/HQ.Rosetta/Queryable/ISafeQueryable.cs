// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HQ.Rosetta.Queryable
{
	/// <summary>
	///     Provides implementation-safe expression predicates when <see cref="IQueryable" /> access is undesirable or
	///     unstable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISafeQueryable<T>
	{
		Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);
		T SingleOrDefault(Expression<Func<T, bool>> predicate);
		Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
		T FirstOrDefault(Expression<Func<T, bool>> predicate);
	}
}