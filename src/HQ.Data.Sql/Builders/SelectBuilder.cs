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
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Extensions;
using TypeKitchen;

namespace HQ.Data.Sql.Builders
{
	public static class SelectBuilder
	{
		public static string Select(this ISqlDialect d, string table, string schema = "")
		{
			return Pooling.StringBuilderPool.Scoped(
				sb => { sb.Append("SELECT * FROM ").AppendTable(d, table, schema); });
		}

		public static string Select(this ISqlDialect d, string table, string schema, List<string> keys)
		{
			return Pooling.StringBuilderPool.Scoped(sb =>
			{
				sb.Append("SELECT * FROM ").AppendTable(d, table, schema).AppendWhereClause(d, keys);
			});
		}

		public static string Select(this ISqlDialect d, string table, string schema, List<string> columns,
			List<string> keys)
		{
			return Pooling.StringBuilderPool.Scoped(sb =>
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

		public static string Select(this ISqlDialect d, string table, string schema, List<string> columns,
			List<string> keys, List<string> parameters)
		{
			return Pooling.StringBuilderPool.Scoped(sb =>
			{
				sb.Append("SELECT ");

				for (var i = 0; i < columns.Count; i++)
				{
					sb.AppendName(d, columns[i]);
					if (i < columns.Count - 1)
						sb.Append(", ");
				}

				sb.Append(" FROM ").AppendTable(d, table, schema);

				sb.AppendWhereClause(d, keys, parameters);
			});
		}

		public static string Select(this ISqlDialect d, IDataDescriptor descriptor, string table, string schema,
			List<string> columns, List<string> keys, List<string> parameters)
		{
			return Pooling.StringBuilderPool.Scoped(sb =>
			{
				if (descriptor != null && !d.BeforeSelect(descriptor, sb))
					return;

				sb.Append("SELECT ");

				if (!d.BeforeSelectColumns(descriptor, sb, columns))
					return;

				for (var i = 0; i < columns.Count; i++)
				{
					sb.AppendName(d, columns[i]);
					if (i < columns.Count - 1)
						sb.Append(", ");
				}

				sb.Append(" FROM ").AppendTable(d, table, schema);

				sb.AppendWhereClause(descriptor, d, keys, parameters);
			});
		}
	}
}