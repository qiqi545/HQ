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

using System.Linq;
using HQ.DotLiquid;
using HQ.Lingo.Builders;
using HQ.Lingo.Descriptor;

namespace HQ.Lingo.Queries
{
    partial class SqlBuilder
    {
        public static Query Delete<T>(dynamic where = null)
        {
            var descriptor = GetDescriptor<T>();
            return Delete(descriptor, where);
        }

        public static Query Delete(IDataDescriptor descriptor, object where)
        {
            var hash = Hash.FromAnonymousObject(where);
            var keys = hash.Keys.Intersect(Dialect.ResolveColumnNames(descriptor)).ToArray();
            var sql = Dialect.DeleteFrom(descriptor.Table, descriptor.Schema, keys);
            var parameters = keys.ToDictionary(key => $"{Dialect.Parameter}{key}", key => hash[key]);
            return new Query(sql, parameters);
        }
    }
}
