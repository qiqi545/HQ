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
using HQ.Data.Sql.Builders;
using HQ.Data.Sql.Descriptor;
using TypeKitchen;

namespace HQ.Data.Sql.Queries
{
    partial class SqlBuilder
    {
        public static Query Delete<T>(dynamic where = null)
        {
            return Delete(GetDescriptor<T>(), where);
        }

        public static Query Delete<T>(IDataDescriptor instance, dynamic where = null)
        {
            var descriptor = GetDescriptor<T>();

            var hash = ReadAccessor.Create(instance?.GetType()).AsReadOnlyDictionary(instance);
            var hashKeysRewrite = hash.Keys.ToDictionary(k => Dialect.ResolveColumnName(descriptor, k), v => v);

            IReadOnlyDictionary<string, object> whereHash;
            List<string> whereFilter;
            if (where == null)
            {
                // WHERE is derived from the instance's primary key
                var keys = Dialect.ResolveKeyNames(descriptor);
                whereFilter = keys.Intersect(hashKeysRewrite.Keys).ToList();
                whereHash = hash;
            }
            else
            {
                // WHERE is explicitly provided 
                var accessor = ReadAccessor.Create(where);
                whereHash = ReadAccessorExtensions.AsReadOnlyDictionary(accessor, where);
                var whereHashKeysRewrite = whereHash.Keys.ToDictionary(k => Dialect.ResolveColumnName(descriptor, k), v => v);
                whereFilter = Dialect.ResolveColumnNames(descriptor).Intersect(whereHashKeysRewrite.Keys).ToList();
            }

            return Delete(descriptor, whereFilter, whereHash);
        }

        public static Query Delete(object instance)
        {
            return Delete(GetDescriptor(instance.GetType()), instance);
        }

        public static Query Delete(IDataDescriptor descriptor, object instance)
        {
            var accessor = ReadAccessor.Create(instance);
            var whereHash = accessor.AsReadOnlyDictionary(instance);
            var whereHashKeyRewrite = whereHash.Keys.ToDictionary(k => Dialect.ResolveColumnName(descriptor, k), v => v);

            var whereFilter = Dialect.ResolveColumnNames(descriptor).Intersect(whereHashKeyRewrite.Keys).ToList();
            var whereParams = whereFilter.ToDictionary(key => $"{whereHashKeyRewrite[key]}", key => whereHash[whereHashKeyRewrite[key]]);
            var whereParameters = whereParams.Keys.ToList();

            var sql = Dialect.Delete(descriptor, Dialect.ResolveTableName(descriptor), descriptor.Schema, whereFilter, whereParameters);
            return new Query(sql, whereParams);
        }

        private static Query Delete(IDataDescriptor descriptor, List<string> whereFilter, IReadOnlyDictionary<string, object> whereHash)
        {
            var whereHashKeyRewrite = whereHash.Keys.ToDictionary(k => Dialect.ResolveColumnName(descriptor, k), v => v);
            var whereParams = whereFilter.ToDictionary(key => $"{whereHashKeyRewrite[key]}", key => whereHash[whereHashKeyRewrite[key]]);
            var whereParameters = whereParams.Keys.ToList();

            var sql = Dialect.Delete(descriptor, Dialect.ResolveTableName(descriptor), descriptor.Schema, whereFilter, whereParameters);
            return new Query(sql, whereParams);
        }
    }
}
