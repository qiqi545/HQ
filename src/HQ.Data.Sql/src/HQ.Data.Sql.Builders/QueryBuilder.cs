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
using System.Linq.Expressions;
using System.Text;
using HQ.Common;
using HQ.Data.Contracts;
using HQ.Data.Sql.Dialects;
using TypeKitchen;

namespace HQ.Data.Sql.Builders
{
    public static class QueryBuilder
    {
        public static string Query(this ISqlDialect d, string table, string schema, IList<string> columns,
            IList<Filter> filters, IList<Projection> projections, IList<Sort> orderByColumns)
        {
            return Pooling.StringBuilderPool.Scoped(sb =>
            {
                AppendQuery(sb, d, table, schema, columns, filters, projections, orderByColumns);
            });
        }

        public static string Query(this ISqlDialect d, string table, string schema, IList<string> columns,
            IList<Filter> filters, IList<Projection> projections, IList<Tuple<string, string, bool>> orderByColumns)
        {
            return Pooling.StringBuilderPool.Scoped(sb =>
            {
                AppendQuery(sb, d, table, schema, columns, filters, projections, orderByColumns);
            });
        }

        public static string Query<T>(this ISqlDialect d, string table, string schema, IList<string> columns,
            IList<Filter> filters, IList<Projection> projections, Expression<Func<T, object>>[] orderByColumns)
        {
            return Pooling.StringBuilderPool.Scoped(sb =>
            {
                AppendQuery(sb, d, table, schema, columns, filters, projections, orderByColumns);
            });
        }

        internal static void AppendQuery(StringBuilder sb, ISqlDialect d, string table, string schema,
            IList<string> columns, IList<Filter> filters, IList<Projection> projections, IList<Sort> orderByColumns)
        {
            ProjectionBuilder.AppendSelect(sb, d, table, schema, columns, projections, filters);

            if (filters?.Count > 0)
            {
                sb.Append(' ');
                FilterBuilder.AppendWhere(sb, d, filters);
            }

            if (orderByColumns?.Count > 0)
            {
                sb.Append(' ');
                OrderByBuilder.AppendOrderBy(sb, d, orderByColumns);
            }
        }

        internal static void AppendQuery(StringBuilder sb, ISqlDialect d, string table, string schema,
            IList<string> columns, IList<Filter> filters, IList<Projection> projections,
            IList<Tuple<string, string, bool>> orderByColumns)
        {
            ProjectionBuilder.AppendSelect(sb, d, table, schema, columns, projections, filters);

            if (filters?.Count > 0)
            {
                sb.Append(' ');
                FilterBuilder.AppendWhere(sb, d, filters);
            }

            if (orderByColumns?.Count > 0)
            {
                sb.Append(' ');
                OrderByBuilder.AppendOrderBy(sb, d, orderByColumns);
            }
        }

        internal static void AppendQuery<T>(StringBuilder sb, ISqlDialect d, string table, string schema,
            IList<string> columns, IList<Filter> filters, IList<Projection> projections,
            Expression<Func<T, object>>[] orderByColumns)
        {
            ProjectionBuilder.AppendSelect(sb, d, table, schema, columns, projections, filters);

            if (filters?.Count > 0)
            {
                sb.Append(' ');
                FilterBuilder.AppendWhere(sb, d, filters);
            }

            if (orderByColumns?.Length > 0)
                OrderByBuilder.AppendOrderBy(sb, d, orderByColumns);
        }
    }
}
