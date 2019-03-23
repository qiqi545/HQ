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

using System.Collections.Generic;
using System.Linq;
using HQ.Common;
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Extensions;

namespace HQ.Data.Sql.Builders
{
    public static class InsertBuilder
    {
        public static string InsertInto(this ISqlDialect d, string table, string schema, List<string> columns,
            bool returnKeys)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                sb.Append("INSERT INTO ");

                sb.AppendTable(d, table, schema).Append(" (");
                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    sb.AppendName(d, column);
                    if (i < columns.Count - 1)
                        sb.Append(", ");
                }

                sb.Append(") ");
                if (returnKeys &&
                    d.TryFetchInsertedKey(FetchInsertedKeyLocation.BeforeValues, out var fetchBeforeValues))
                    sb.Append(fetchBeforeValues).Append(" ");

                sb.Append("VALUES (");

                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    sb.AppendParameter(d, column);
                    if (i < columns.Count - 1)
                        sb.Append(",");
                }

                sb.Append(")");

                if (returnKeys &&
                    d.TryFetchInsertedKey(FetchInsertedKeyLocation.AfterStatement, out var fetchAfterStatement))
                    sb.Append("; ").Append(fetchAfterStatement);
            });
        }

        public static string InsertInto(this ISqlDialect d, IDataDescriptor descriptor, string table, string schema,
            List<string> columns, bool returnKeys)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                if (!d.BeforeInsert(descriptor, sb))
                    return;

                sb.Append("INSERT INTO ");
                sb.AppendTable(d, table, schema).Append(" (");

                if (!d.BeforeInsertColumns(descriptor, sb, columns))
                    return;

                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    sb.AppendName(d, column);
                    if (i < columns.Count - 1)
                        sb.Append(", ");
                }

                sb.Append(") ");

                if (returnKeys &&
                    d.TryFetchInsertedKey(FetchInsertedKeyLocation.BeforeValues, out var fetchBeforeValues))
                    sb.Append(fetchBeforeValues).Append(" ");

                sb.Append("VALUES (");

                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    sb.AppendParameter(d, column);
                    if (i < columns.Count - 1)
                        sb.Append(",");
                }

                sb.Append(")");

                if (returnKeys &&
                    d.TryFetchInsertedKey(FetchInsertedKeyLocation.AfterStatement, out var fetchAfterStatement))
                    sb.Append("; ").Append(fetchAfterStatement);
            });
        }

        public static string InsertInto(this ISqlDialect d, string table, string schema, List<PropertyToColumn> columns,
            bool returnKeys)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                sb.Append("INSERT INTO ");
                sb.AppendTable(d, table, schema).Append(" (");

                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    sb.AppendName(d, column.ColumnName);
                    if (i < columns.Count - 1)
                        sb.Append(", ");
                }

                sb.Append(") ");
                if (returnKeys &&
                    d.TryFetchInsertedKey(FetchInsertedKeyLocation.BeforeValues, out var fetchBeforeValues))
                    sb.Append(fetchBeforeValues).Append(" ");

                sb.Append("VALUES (");

                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    sb.AppendParameter(d, column.ColumnName);
                    if (i < columns.Count - 1)
                        sb.Append(",");
                }

                sb.Append(")");

                if (returnKeys &&
                    d.TryFetchInsertedKey(FetchInsertedKeyLocation.AfterStatement, out var fetchAfterStatement))
                    sb.Append("; ").Append(fetchAfterStatement);
            });
        }

        public static string InsertInto(this ISqlDialect d, IDataDescriptor descriptor, string table, string schema,
            List<PropertyToColumn> columns, bool returnKeys)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                if (!d.BeforeInsert(descriptor, sb))
                    return;

                sb.Append("INSERT INTO ");
                sb.AppendTable(d, table, schema).Append(" (");

                if (!d.BeforeInsertColumns(descriptor, sb, columns.Select(x => x.ColumnName).ToList()))
                    return;

                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    sb.AppendName(d, column.ColumnName);
                    if (i < columns.Count - 1)
                        sb.Append(", ");
                }

                sb.Append(") ");
                if (returnKeys &&
                    d.TryFetchInsertedKey(FetchInsertedKeyLocation.BeforeValues, out var fetchBeforeValues))
                    sb.Append(fetchBeforeValues).Append(" ");

                sb.Append("VALUES (");

                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    sb.AppendParameter(d, column.ColumnName);
                    if (i < columns.Count - 1)
                        sb.Append(",");
                }

                sb.Append(")");

                if (returnKeys &&
                    d.TryFetchInsertedKey(FetchInsertedKeyLocation.AfterStatement, out var fetchAfterStatement))
                    sb.Append("; ").Append(fetchAfterStatement);
            });
        }
    }
}
