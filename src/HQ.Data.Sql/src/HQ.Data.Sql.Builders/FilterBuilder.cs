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
using System.Text;
using HQ.Common;
using HQ.Data.Contracts;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Extensions;
using TypeKitchen;

namespace HQ.Data.Sql.Builders
{
    public static class FilterBuilder
    {
        public static string Where(this ISqlDialect d, FilterOptions filterOptions)
        {
            return d.Where(filterOptions.Fields);
        }

        public static string Where(this ISqlDialect d, List<Filter> filters)
        {
            var clauses = string.Join(" AND ", filters.Enumerate(f => d.Where(f)));

            return clauses;
        }

        public static string Where(this ISqlDialect d, params Filter[] filters)
        {
            return Pooling.StringBuilderPool.Scoped(sb => { AppendWhere(sb, d, filters); });
        }

        internal static void AppendWhere(StringBuilder sb, ISqlDialect d, IList<Filter> filters)
        {
            sb.Append("WHERE ");

            var joins = 0;
            for (var i = 0; i < filters.Count; i++)
            {
                AppendFilterClause(d, sb, filters[i], ref joins);
                if (i < filters.Count - 1)
                    sb.Append(" AND ");
            }
        }

        private static void AppendFilterClause(this ISqlDialect d, StringBuilder sb, Filter f, ref int joins)
        {
            switch (f.Type)
            {
                case FilterType.Scalar:
                    AppendFilterScalarClause(d, sb, f);
                    break;
                case FilterType.Join:
                    AppendFilterJoinClause(d, sb, f, ref joins);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void AppendFilterJoinClause(this ISqlDialect d, StringBuilder sb, Filter f, ref int joins)
        {
            sb.Append(Constants.Sql.ChildAlias).Append(joins).Append('.').AppendName(d, f.Field);

            var isNullCheck = f.IsNullCheck();
            AppendOperator(sb, f, isNullCheck);
            if (isNullCheck)
                return;

            AppendFilterValue(d, sb, f);
        }

        private static void AppendFilterScalarClause(this ISqlDialect d, StringBuilder sb, Filter f)
        {
            sb.Append(Constants.Sql.ParentAlias).Append('.').AppendName(d, f.Field);

            var isNullCheck = f.IsNullCheck();
            AppendOperator(sb, f, isNullCheck);
            if (isNullCheck)
                return;

            AppendFilterValue(d, sb, f);
        }

        private static void AppendOperator(StringBuilder sb, Filter f, bool isNullCheck)
        {
            switch (f.Operator)
            {
                case FilterOperator.Equal:
                    sb.Append(isNullCheck ? " IS NULL" : " = ");
                    break;
                case FilterOperator.NotEqual:
                    sb.Append(isNullCheck ? " IS NOT NULL" : " <> ");
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private static void AppendFilterValue(ISqlDialect d, StringBuilder sb, Filter f)
        {
            if (f.IsParameter(d))
                sb.Append(f.Value);
            else if (f.Value is string || f.Value is DateTimeOffset || f.Value is DateTime)
                sb.AppendQuoted(d, f.Value);
            else
                sb.Append(f.Value);
        }

        public static bool IsNullCheck(this Filter f)
        {
            var isNullCheck = (f.Value as string)?.Equals("NULL", StringComparison.OrdinalIgnoreCase);
            return isNullCheck.GetValueOrDefault();
        }

        public static bool IsParameter(this Filter f, ISqlDialect d)
        {
            if (f.Value == null)
                return false;
            var startsWith = (f.Value as string)?.StartsWith($"{d.Parameter}");
            return startsWith.GetValueOrDefault();
        }

        public static object EqualTo(this object clause)
        {
            return clause;
        }

        public static object NotEqualTo(this object clause)
        {
            return clause;
        }
    }
}
