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
using System.Data;
using System.Linq.Expressions;
using Dapper;
using HQ.Lingo.Queries;
using HQ.Rosetta;

namespace HQ.Lingo.Dapper
{
    // TODO codegen multiple extensions or use runtime inference

    partial class DapperExtensions
    {
        public static IEnumerable<TParent> Query<TParent, TChild1, TChild2, TParentKey>(this IDbConnection connection,
            Func<TParent, TParentKey> parentKeySelector, Action<TParent, TChild1> child1Setter,
            Func<TParent, IList<TChild2>> child2Selector,
            dynamic data, IList<Filter> filters, IList<Projection> projections,
            IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, params Expression<Func<TParent, object>>[] orderBy) where TParent : class
        {
            Query query = SqlBuilder.Select<TParent>(data, filters, projections, orderBy);
            var result = connection.QueryOneAndMany(query.Sql, parentKeySelector, child1Setter, child2Selector,
                query.Parameters.Prepare(), transaction, buffered, commandTimeout: commandTimeout,
                commandType: commandType);
            return result;
        }

        internal static IEnumerable<TParent> QueryOneAndMany<TParent, TChild1, TChild2, TParentKey>(
            this IDbConnection connection, string sql,
            Func<TParent, TParentKey> parentKeySelector, Action<TParent, TChild1> child1Setter,
            Func<TParent, IList<TChild2>> child2Selector,
            dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id",
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var cache = new Dictionary<TParentKey, TParent>();
            connection.Query<TParent, TChild1, TChild2, TParent>(sql, (parent, child1, child2) =>
            {
                var parentKey = parentKeySelector(parent);
                if (!cache.TryGetValue(parentKey, out var cachedParent))
                    cache.Add(parentKey, cachedParent = parent);
                child1Setter(cachedParent, child1);
                child2Selector(cachedParent).Add(child2);
                return cachedParent;
            }, param as object, transaction, buffered, splitOn, commandTimeout, commandType);

            return cache.Values;
        }
    }
}
