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
using HQ.Common.Helpers;
using HQ.Data.Contracts;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Extensions;
using Humanizer;

namespace HQ.Data.Sql.Builders
{
    public static class ProjectionBuilder
    {
        public static string Select(this ISqlDialect d, string table, string schema, IList<string> columns,
            params Projection[] projections)
        {
            return Select(d, table, schema, columns, projections, null);
        }

        public static string Select(this ISqlDialect d, string table, string schema, IList<string> columns,
            IList<Projection> projections, IList<Filter> filters)
        {
            return StringBuilderPool.Scoped(
                sb => { AppendSelect(sb, d, table, schema, columns, projections, filters); });
        }

        internal static void AppendSelect(StringBuilder sb, ISqlDialect d, string table, string schema,
            IList<string> columns, IList<Projection> projections, IList<Filter> filters)
        {
            sb.Append("SELECT ");

            for (var i = 0; i < columns.Count; i++)
            {
                sb.Append(Constants.Sql.ParentAlias).Append('.');
                sb.AppendName(d, columns[i]);
                if (i < columns.Count - 1)
                    sb.Append(", ");
            }

            var joins = 0;
            foreach (var _ in projections)
            {
                sb.Append(", ").Append(Constants.Sql.ParentAlias).Append(joins).Append(".*");
                joins++;
            }

            sb.Append(" FROM ").AppendTable(d, table, schema).Append(' ').Append(Constants.Sql.ParentAlias).Append(' ');

            joins = 0;
            for (var i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                if (filter.Type != FilterType.Join)
                    continue;

                var title = filter.Field.Humanize(LetterCasing.Title);

                sb.Append("LEFT JOIN ")
                    .AppendName(d, title)
                    .Append(Constants.Sql.ChildAlias).Append(joins).Append(" ON ")
                    .Append(Constants.Sql.ChildAlias).Append(joins).Append('.').Append(table).Append("Id").Append(" = ")
                    .Append(Constants.Sql.ParentAlias).Append('.').AppendName(d, "Id");

                if (i < projections.Count - 1)
                    sb.Append(' ');

                joins++;
            }

            joins = 0;
            for (var i = 0; i < projections.Count; i++)
            {
                var expansion = projections[i];

                if (expansion.Type == ProjectionType.Scalar)
                    continue;

                var title = expansion.Field.Humanize(LetterCasing.Title);

                switch (expansion.Type)
                {
                    case ProjectionType.OneToOne:

                        sb.Append("LEFT JOIN ").AppendName(d, title).Append(' ')
                            .Append(Constants.Sql.ParentAlias).Append(joins).Append(" ON ")
                            .Append(Constants.Sql.ParentAlias).Append(joins).Append('.').AppendName(d, "Id")
                            .Append(" = ")
                            .Append(Constants.Sql.ParentAlias).Append('.')
                            .Append(d.StartIdentifier).Append(title).Append("Id").Append(d.EndIdentifier);

                        break;
                    case ProjectionType.OneToMany:

                        var singularRoute = title.Singularize();
                        var name = singularRoute;

                        sb.Append("LEFT JOIN ").Append(d.StartIdentifier).Append(table).Append(name)
                            .Append(d.EndIdentifier)
                            .Append(' ')
                            .Append(Constants.Sql.ChildAlias).Append(joins).Append(" ON ")
                            .Append(Constants.Sql.ChildAlias).Append(joins).Append('.').Append(d.StartIdentifier)
                            .Append(table)
                            .Append("Id").Append(d.EndIdentifier).Append(" = ").Append(Constants.Sql.ParentAlias)
                            .Append('.')
                            .AppendName(d, "Id")
                            .Append(' ');

                        sb.Append("LEFT JOIN ").AppendName(d, name).Append(' ')
                            .Append(Constants.Sql.ParentAlias).Append(joins).Append(" ON ")
                            .Append(Constants.Sql.ParentAlias).Append(joins).Append('.').AppendName(d, "Id")
                            .Append(" = ")
                            .Append(Constants.Sql.ChildAlias).Append(joins).Append('.')
                            .Append(d.StartIdentifier).Append(name).Append("Id").Append(d.EndIdentifier);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (i < projections.Count - 1)
                    sb.Append(' ');
                joins++;
            }
        }
    }
}
