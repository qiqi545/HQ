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
using System.Text;
using HQ.Lingo.Dialects;

namespace HQ.Lingo.Extensions
{
    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendName(this StringBuilder sb, ISqlDialect d, object value)
        {
            return sb.Append(d.StartIdentifier).Append(value).Append(d.EndIdentifier);
        }

        public static StringBuilder AppendQuoted(this StringBuilder sb, ISqlDialect d, object value)
        {
            return sb.Append(d.Quote).Append(value).Append(d.Quote);
        }

        public static StringBuilder AppendParameter(this StringBuilder sb, ISqlDialect d, object value)
        {
            return sb.Append(d.Parameter).Append(value);
        }

        public static StringBuilder AppendTable(this StringBuilder sb, ISqlDialect d, string table, string schema)
        {
            if (!string.IsNullOrWhiteSpace(schema))
                sb.AppendName(d, schema).Append('.');
            sb.AppendName(d, table);
            return sb;
        }

        public static StringBuilder AppendWhereClause(this StringBuilder sb, ISqlDialect d, IList<string> keys)
        {
            if (keys != null)
                for (var i = 0; i < keys.Count; i++)
                {
                    sb.Append(i == 0 ? " WHERE " : " AND ");
                    var key = keys[i];
                    sb.AppendName(d, key).Append(" = ").AppendParameter(d, key);
                }

            return sb;
        }

        public static StringBuilder AppendWhereClause(this StringBuilder sb, ISqlDialect d, IList<string> keys,
            IList<string> parameters)
        {
            if (keys != null)
                for (var i = 0; i < keys.Count; i++)
                {
                    sb.Append(i == 0 ? " WHERE " : " AND ");
                    var key = keys[i];
                    sb.AppendName(d, key).Append(" = ").AppendParameter(d, parameters[i]);
                }

            return sb;
        }
    }
}
