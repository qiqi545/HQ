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

using HQ.Data.Contracts;
using HQ.Data.Sql.Builders;
using HQ.Data.Sql.Dialects;
using TypeKitchen;

namespace HQ.Data.Sql.Queries
{
    public static class ObjectQueryBuilder
    {
        public static string Build<T>(this ISqlDialect dialect, SortOptions sort = null, FieldOptions fields = null,
            FilterOptions filter = null, ProjectionOptions projections = null)
        {
            return Pooling.StringBuilderPool.Scoped(sb =>
            {
                // SELECT * FROM ...
                sb.Append(ProjectionBuilder.Select<T>(dialect, fields, projections));

                // WHERE ...
                if (filter?.Fields.Count > 0) sb.Append($" {dialect.Where(filter)}");

                // ORDER BY ...
                if (sort?.Fields.Count > 0)
                    sb.Append($" {SortingBuilder.OrderBy(dialect, sort)}");
            });
        }
    }
}
