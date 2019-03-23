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
using DotLiquid;
using HQ.Data.Contracts;
using HQ.Data.Sql.Builders;
using HQ.Data.Sql.Descriptor;

namespace HQ.Data.Sql.Queries
{
    partial class SqlBuilder
    {
        public static Query Select<T>(dynamic data, List<Filter> filters, List<Projection> projections,
            params Expression<Func<T, object>>[] orderBy)
        {
            var descriptor = GetDescriptor<T>();
            Query query = Select(descriptor, data, null, filters, projections, orderBy);
            if (orderBy?.Length > 0)
                query.Sql = Dialect.OrderBy(query.Sql, orderBy);
            return query;
        }

        private static Query Select<T>(IDataDescriptor descriptor, dynamic data, List<string> columnFilter,
            List<Filter> filters, List<Projection> projections, Expression<Func<T, object>>[] orderBy)
        {
            var columns = columnFilter ?? Dialect.ResolveColumnNames(descriptor).ToList();
            var sql = Dialect.Query(descriptor.Table, descriptor.Schema, columns, filters, projections, orderBy);

            IDictionary<string, object> whereHash = Hash.FromAnonymousObject(data);
            var whereFilter = Dialect.ResolveColumnNames(descriptor).Intersect(whereHash.Keys).ToList();
            var parameters = whereFilter.ToDictionary(key => $"{Dialect.Parameter}{key}", key => whereHash[key]);

            return new Query(sql, parameters);
        }
    }
}
