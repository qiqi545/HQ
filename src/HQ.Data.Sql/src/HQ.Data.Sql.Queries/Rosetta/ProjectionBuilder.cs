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
using HQ.Common.Extensions;
using HQ.Common.Helpers;
using HQ.Data.Sql.Dialects;
using HQ.Data.Contracts;

namespace HQ.Data.Sql.Queries.Rosetta
{
    public static class ProjectionBuilder
    {
        public static string Select<T>(ISqlDialect dialect, FieldOptions fields, ProjectionOptions projections)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                // SELECT * FROM ...
                sb.Append($"SELECT {BuildFields<T>(dialect, fields, projections)} " +
                          $"FROM {dialect.StartIdentifier}{typeof(T).Name}{dialect.EndIdentifier} r ");

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
                            /*
                                INNER JOIN Role r0 ON r0.Id = x0.RoleId
                            */
                            sb.Append($"INNER JOIN {name} r{joins} ON r{joins}.Id = x{joins}.{name}Id ");
                            break;
                        case ProjectionType.OneToMany:
                            /*
                                INNER JOIN UserRole x0 ON x0.UserId = r.Id
                                INNER JOIN Role r0 ON r0.Id = x0.RoleId
                            */
                            sb.Append(
                                $"INNER JOIN {typeof(T).Name}{name} x{joins} ON x{joins}.{typeof(T).Name}Id = r.Id ");
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

        private static string BuildFields<T>(ISqlDialect dialect, FieldOptions options, ProjectionOptions projections)
        {
            var source = BuildFieldSource<T>(options);

            return StringBuilderPool.Scoped(sb =>
            {
                sb.Append(string.Join(", ",
                    source.Select(a => $"r.{dialect.StartIdentifier}{a}{dialect.EndIdentifier}")));

                if (projections?.Fields == null)
                    return;

                var joins = 0;
                foreach (var projection in projections?.Fields)
                {
                    sb.Append($", r{joins}.*");
                    joins++;
                }
            });
        }

        private static IOrderedEnumerable<string> BuildFieldSource<T>(FieldOptions options)
        {
            IOrderedEnumerable<string> source;
            if (options?.Fields == null || options.Fields.Count == 0)
            {
                var memberSet = GetProjectionMembersNames(typeof(T));

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
