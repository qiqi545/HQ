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
using HQ.Lingo.Dialects;

namespace HQ.Lingo.Queries
{
    // TODO no ToList via StringBank
    // TODO remove hashKeysRewrite

    partial class SqlBuilder
    {
        public static Query Insert<T>(T entity)
        {
            return Insert(entity, GetDescriptor<T>());
        }

        public static Query Insert<T>(T instance, IDataDescriptor descriptor)
        {
            return Insert(instance as object, descriptor);
        }

        public static Query Insert(object instance)
        {
            return Insert(instance, GetDescriptor(instance.GetType()));
        }

        public static Query Insert(object instance, IDataDescriptor descriptor)
        {
            var columns = Dialect.ResolveColumnNames(descriptor, ColumnScope.Inserted).ToList();
            var sql = Dialect.InsertInto(Dialect.ResolveTableName(descriptor), descriptor.Schema, columns, false);
            var hash = Hash.FromAnonymousObject(instance, true);
            var hashKeysRewrite = hash.Keys.ToDictionary(k => Dialect.ResolveColumnName(descriptor, k), v => v);
            var keys = columns.Intersect(hashKeysRewrite.Keys);
            var parameters = keys.ToDictionary(key => $"{Dialect.Parameter}{key}", key => hash[hashKeysRewrite[key]]);

            return new Query(sql, parameters);
        }
    }
}
