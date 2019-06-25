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

using HQ.Common;
using TypeKitchen;

namespace HQ.Platform.Schema.Extensions
{
    public static class SchemaExtensions
    {
        public static string FullTypeString(this Models.Schema schema, string ns = null)
        {
            return Pooling.StringBuilderPool.Scoped(sb =>
            {
                sb.Append(ns ?? schema?.Namespace ?? Constants.Schemas.DefaultNamespace);
                sb.Append('.').Append(schema.TypeString());
            });
        }

        public static string TypeString(this Models.Schema schema)
        {
            return $"{schema.Name.Identifier()}";
        }
    }
}
