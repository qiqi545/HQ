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
using System.Linq;
using HQ.Common;
using HQ.Data.Contracts;
using HQ.Data.Sql.Dialects;
using TypeKitchen;

namespace HQ.Data.Sql.Queries
{
	public static class ProjectionBuilder
	{
		public static string Select<T>(this ISqlDialect dialect, FieldOptions fields, ProjectionOptions projections)
		{
			return Select(dialect, typeof(T), fields, projections);
		}

		public static string Select(this ISqlDialect dialect, Type type, FieldOptions fields,
			ProjectionOptions projections)
		{
			return Pooling.StringBuilderPool.Scoped(sb =>
			{
				// SELECT * FROM ...
				sb.Append($"SELECT {BuildFields(dialect, type, fields, projections)} " +
				          $"FROM {dialect.StartIdentifier}{type.Name}{dialect.EndIdentifier} r ");

				if (projections?.Fields == null)
					return;

				// INNER JOIN...
				var joins = 0;
				foreach (var projection in projections.Fields)
				{
					var name = projection.Field.ToTitleCase();
					if (name.EndsWith("s"))
						name = name.Substring(0, name.Length - 1);

					switch (projection.Type)
					{
						case ProjectionType.OneToOne:
							sb.Append($"INNER JOIN {name} r{joins} ON r{joins}.Id = x{joins}.{name}Id ");
							break;
						case ProjectionType.OneToMany:
							sb.Append($"INNER JOIN {type.Name}{name} x{joins} ON x{joins}.{type.Name}Id = r.Id ");
							sb.Append($"INNER JOIN {name} r{joins} ON r{joins}.Id = x{joins}.{name}Id ");
							break;
						case ProjectionType.Scalar:
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					joins++;
				}
			});
		}

		private static string BuildFields(this ISqlDialect dialect, Type type, FieldOptions options,
			ProjectionOptions projections)
		{
			var source = BuildFieldSource(type, options);

			return Pooling.StringBuilderPool.Scoped(sb =>
			{
				sb.Append(string.Join(", ",
					source.Select(a => $"r.{dialect.StartIdentifier}{a}{dialect.EndIdentifier}")));

				if (projections?.Fields == null)
					return;

				var joins = 0;
				foreach (var projection in projections.Fields)
				{
					sb.Append($", r{joins}.*");
					joins++;
				}
			});
		}

		private static IOrderedEnumerable<string> BuildFieldSource(Type type, FieldOptions options)
		{
			IOrderedEnumerable<string> source;
			if (options?.Fields == null || options.Fields.Count == 0)
			{
				var memberSet = GetProjectionMembersNames(type);

				source = memberSet.OrderBy(a => a);
			}
			else
			{
				source = options.Fields.Select(a => $"{a}").OrderBy(a => a);
			}

			return source;
		}

		private static IEnumerable<string> GetProjectionMembersNames(Type type)
		{
			// TODO get property names from a cache
			var descriptor = SqlBuilder.GetDescriptor(type);
			return descriptor.Inserted.Select(x => x.Property.Name);
		}
	}
}