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
using HQ.Common.Helpers;
using HQ.Lingo.Builders.Extensions;
using HQ.Lingo.Dialects;

namespace HQ.Lingo.Builders
{
    public static class SelectBuilder
    {
        public static string Select(this ISqlDialect d, string table, string schema = "")
        {
            return StringBuilderPool.Scoped(sb => { sb.Append("SELECT * FROM ").AppendTable(d, table, schema); });
        }

        public static string Select(this ISqlDialect d, string table, string schema, IList<string> keys)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                sb.Append("SELECT * FROM ").AppendTable(d, table, schema).AppendWhereClause(d, keys);
            });
        }

        public static string Select(this ISqlDialect d, string table, string schema, IList<string> columns,
            IList<string> keys)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                sb.Append("SELECT ");

                for (var i = 0; i < columns.Count; i++)
                {
                    sb.AppendName(d, columns[i]);
                    if (i < columns.Count - 1)
                        sb.Append(", ");
                }

                sb.Append(" FROM ").AppendTable(d, table, schema).AppendWhereClause(d, keys);
            });
        }

        public static string Select(this ISqlDialect d, string table, string schema, IList<string> columns,
            IList<string> keys, IList<string> parameters)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                sb.Append("SELECT ");

                for (var i = 0; i < columns.Count; i++)
                {
                    sb.AppendName(d, columns[i]);
                    if (i < columns.Count - 1)
                        sb.Append(", ");
                }

                sb.Append(" FROM ").AppendTable(d, table, schema).AppendWhereClause(d, keys, parameters);
            });
        }
    }
}
