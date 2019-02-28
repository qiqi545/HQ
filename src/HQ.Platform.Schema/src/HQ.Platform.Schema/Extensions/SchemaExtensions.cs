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
using HQ.Common;
using HQ.Common.Helpers;
using HQ.Platform.Schema.Models;

namespace HQ.Platform.Schema.Extensions
{
    public static class SchemaExtensions
    {
        public static string Prefix(this Models.Schema schema, string ns = null)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                sb.Append(ns ?? schema?.Self?.Namespace ?? Constants.Schema.DefaultNamespace);
                var version = schema?.Self?.Version ?? 0;
                if (version == 0)
                {
                    return;
                }

                sb.Append($".V{version}");
            });
        }

        public static string Label(this Models.Schema schema, string ns = null)
        {
            return schema?.Name?.Label();
        }

        public static string FullTypeString(this Models.Schema schema, string ns = null)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                sb.Append(ns ?? schema?.Self?.Namespace ?? Constants.Schema.DefaultNamespace);
                if (schema?.Self?.Version != 0)
                {
                    var version = schema?.Self?.Version ?? 0;
                    if (version != 0)
                    {
                        sb.Append(".V").Append(version);
                    }
                }

                sb.Append('.').Append(schema.TypeString());
            });
        }

        public static string TypeString(this Models.Schema schema)
        {
            return $"{schema.Name.Identifier()}";
        }

        public static string VersionString(this Models.Schema schema)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                sb.Append(schema?.Self?.Namespace ?? Constants.Schema.DefaultNamespace);
                if (schema?.Self?.Version == 0)
                {
                    return;
                }

                var version = schema?.Self?.Version ?? 0;
                if (version == 0)
                {
                    return;
                }

                sb.Append($".V{version}");
            });
        }

        public static IEnumerable<Models.Schema> GetOneToMany(this Models.Schema schema,
            Dictionary<string, Models.Schema> map)
        {
            var self = schema.FullTypeString();

            foreach (var property in schema.Properties)
            {
                if (property.Rel != PropertyRelationship.OneToMany)
                {
                    continue;
                }

                var propertyTypeString = property.FullTypeString(map, true);

                foreach (var entry in map)
                {
                    var candidate = entry.Value.FullTypeString();
                    if (self == candidate)
                    {
                        continue;
                    }

                    if (candidate == propertyTypeString)
                    {
                        yield return entry.Value;
                    }
                }
            }
        }
    }
}
