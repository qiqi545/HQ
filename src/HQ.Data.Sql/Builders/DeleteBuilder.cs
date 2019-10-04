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
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Extensions;
using TypeKitchen;

namespace HQ.Data.Sql.Builders
{
	public static class DeleteBuilder
	{
		public static string Delete(this ISqlDialect d, string table, string schema)
		{
			return Pooling.StringBuilderPool.Scoped(sb =>
			{
				sb.Append("DELETE FROM ");
				sb.AppendTable(d, table, schema);
			});
		}

		public static string Delete(this ISqlDialect d, IDataDescriptor descriptor, string table, string schema)
		{
			return Pooling.StringBuilderPool.Scoped(sb =>
			{
				if (!d.BeforeDelete(descriptor, sb))
					return;

				sb.Append("DELETE FROM ");
				sb.AppendTable(d, table, schema);
			});
		}

		public static string Delete(this ISqlDialect d, string table, string schema, List<string> keys)
		{
			return Pooling.StringBuilderPool.Scoped(sb =>
			{
				sb.Append("DELETE FROM ");
				sb.AppendTable(d, table, schema);
				sb.AppendWhereClause(d, keys);
			});
		}

		public static string Delete(this ISqlDialect d, IDataDescriptor descriptor, string table, string schema,
			List<string> keys)
		{
			return Pooling.StringBuilderPool.Scoped(sb =>
			{
				if (!d.BeforeDelete(descriptor, sb))
					return;

				sb.Append("DELETE FROM ");
				sb.AppendTable(d, table, schema);

				if (!d.BeforeWhere(descriptor, sb, keys))
					return;

				sb.AppendWhereClause(descriptor, d, keys);
			});
		}

		public static string Delete(this ISqlDialect d, string table, string schema, List<string> keys,
			List<string> parameters)
		{
			return Pooling.StringBuilderPool.Scoped(sb =>
			{
				sb.Append("DELETE FROM ");
				sb.AppendTable(d, table, schema);
				sb.AppendWhereClause(d, keys, parameters);
			});
		}

		public static string Delete(this ISqlDialect d, IDataDescriptor descriptor, string table, string schema,
			List<string> keys, List<string> parameters)
		{
			return Pooling.StringBuilderPool.Scoped(sb =>
			{
				if (!d.BeforeDelete(descriptor, sb))
					return;

				sb.Append("DELETE FROM ");
				sb.AppendTable(d, table, schema);

				if (!d.BeforeWhere(descriptor, sb, keys, parameters))
					return;

				sb.AppendWhereClause(descriptor, d, keys, parameters);
			});
		}

		public static string Delete(this ISqlDialect d, string table, string schema, List<PropertyToColumn> keys)
		{
			return Pooling.StringBuilderPool.Scoped(sb =>
			{
				sb.Append("DELETE FROM ");
				sb.AppendTable(d, table, schema);
				sb.AppendWhereClause(d, keys.Select(x => x.ColumnName).ToList());
			});
		}
	}
}