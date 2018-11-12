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
using System.Linq;
using System.Linq.Expressions;
using HQ.DotLiquid;
using HQ.Lingo.Builders;
using HQ.Lingo.Descriptor;
using HQ.Rosetta;

namespace HQ.Lingo.Queries
{
    partial class SqlBuilder
    {
        public static Query Select<T>(dynamic data, IList<Filter> filters, IList<Projection> projections,
            params Expression<Func<T, object>>[] orderBy)
        {
            var descriptor = GetDescriptor<T>();
            Query query = Select(descriptor, data, null, filters, projections, orderBy);
            if (orderBy?.Length > 0)
                query.Sql = Dialect.OrderBy(query.Sql, orderBy);
            return query;
        }

        private static Query Select<T>(IDataDescriptor descriptor, dynamic data, IList<string> columnFilter,
            IList<Filter> filters, IList<Projection> projections, Expression<Func<T, object>>[] orderBy)
        {
            var columns = columnFilter ?? Dialect.ResolveColumnNames(descriptor).ToArray();
            var sql = Dialect.Query(descriptor.Table, descriptor.Schema, columns, filters, projections, orderBy);

            IDictionary<string, object> whereHash = Hash.FromAnonymousObject(data);
            var whereFilter = Dialect.ResolveColumnNames(descriptor).Intersect(whereHash.Keys).ToArray();
            var parameters = whereFilter.ToDictionary(key => $"{Dialect.Parameter}{key}", key => whereHash[key]);

            return new Query(sql, parameters);
        }
    }
}
