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
using HQ.Data.Sql.Extensions;
using TypeKitchen;

namespace HQ.Data.Sql.Builders
{
    public static class OrderByBuilder
    {
        public static string OrderBy(this ISqlDialect dialect, IList<Sort> columns)
        {
            return Pooling.StringBuilderPool.Scoped(sb => { AppendOrderBy(sb, dialect, columns); });
        }

        public static string OrderBy(this ISqlDialect dialect, IList<Tuple<string, string, bool>> columns)
        {
            return Pooling.StringBuilderPool.Scoped(sb => { AppendOrderBy(sb, dialect, columns); });
        }

        public static string OrderBy<T>(this ISqlDialect dialect, string sql,
            params Expression<Func<T, object>>[] orderBy)
        {
            return Pooling.StringBuilderPool.Scoped(sb =>
            {
                sb.Append(sql);
                AppendOrderBy(sb, dialect, orderBy);
            });
        }

        internal static void AppendOrderBy(StringBuilder sb, ISqlDialect dialect, IList<Sort> columns)
        {
            sb.Append("ORDER BY ");
            for (var i = 0; i < columns.Count; i++)
            {
                var sort = columns[i];
                sb.AppendName(dialect, sort.Field).Append(" ");
                sb.Append(sort.Descending ? "DESC" : "ASC");
                if (i < columns.Count - 1)
                    sb.Append(", ");
            }
        }

        internal static void AppendOrderBy(StringBuilder sb, ISqlDialect dialect,
            IList<Tuple<string, string, bool>> columns)
        {
            sb.Append("ORDER BY ");
            for (var i = 0; i < columns.Count; i++)
            {
                var field = columns[i];
                if (field.Item1 != null)
                    sb.Append(field.Item1).Append(".");
                sb.AppendName(dialect, field.Item2).Append(" ");
                sb.Append(field.Item3 ? "DESC" : "ASC");
                if (i < columns.Count - 1)
                    sb.Append(", ");
            }
        }

        internal static void AppendOrderBy<T>(StringBuilder sb, ISqlDialect dialect,
            Expression<Func<T, object>>[] orderBy)
        {
            sb.Append(" ORDER BY ");

            var clauses = orderBy.Length;

#if !CSHARP_7_2
            for (var i = 0; i < orderBy.Length; i++)
                if (orderBy[i] == null)
                    clauses--;
#endif

            for (var i = 0; i < orderBy.Length; i++)
            {
                var entry = orderBy[i];

#if !CSHARP_7_2
                if (entry == null)
                    continue;
#endif

                // explicit case: user typed Field.Asc() or Field.Desc()
                if (entry.Body is MethodCallExpression call && call.Arguments.Count == 1 &&
                    call.Arguments[0] is MemberExpression callMember)
                {
                    sb.AppendName(dialect, callMember.Member.Name);
                    sb.Append(call.Method.Name == "Desc" ? " DESC" : " ASC");
                }
                // implicit case: user typed Field and omitted Asc() or Desc()
                else if (entry.Body is MemberExpression member)
                {
                    sb.AppendName(dialect, member.Member.Name);
                    sb.Append(" ASC");
                }
                // special case: user performed the explicit case, but we have multiple params in C# 7.2, which thunks with conversion operations
                else if (entry.Body is MethodCallExpression callConvert &&
                         callConvert.Arguments.Count == 1 &&
                         callConvert.Arguments[0] is UnaryExpression unary &&
                         unary.Operand is MemberExpression convertMember)
                {
                    sb.AppendName(dialect, convertMember.Member.Name);
                    sb.Append(callConvert.Method.Name == "Desc" ? " DESC" : " ASC");
                }
                // special case: user performed the implicit case, but we have multiple params in C# 7.2, which thunks with conversion operations
                else if (entry.Body is UnaryExpression callConvertUnary &&
                         callConvertUnary.Operand is MemberExpression convertMemberOmitted)
                {
                    sb.AppendName(dialect, convertMemberOmitted.Member.Name);
                    sb.Append(" ASC");
                }
                else
                {
                    throw new NotSupportedException("Ordering by an expression has a strict set of outcomes");
                }

                if (i < clauses - 1)
                    sb.Append(", ");
            }
        }
    }
}
