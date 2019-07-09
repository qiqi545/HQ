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
        public static Query DeleteAll<T>()
        {
            var descriptor = GetDescriptor<T>();
            var sql = Dialect.Delete(Dialect.ResolveTableName(descriptor), descriptor.Schema);
            return new Query(sql);
        }

        public static Query Delete<T>(T instance)
        {
            var accessor = ReadAccessor.Create(instance);
            var descriptor = GetDescriptor<T>();
            var where = new Dictionary<string, object>();
            foreach (var key in descriptor.Keys)
            {
                if(accessor.TryGetValue(instance, key.Property.Name, out var value))
                    where.Add(key.ColumnName, value);
            }
            return Delete(descriptor, where);
        }

        public static Query Delete<T>(dynamic where = null)
        {
            return Delete(GetDescriptor<T>(), where);
        }
        
        public static Query Delete(object instance)
        {
            return Delete(GetDescriptor(instance.GetType()), instance);
        }

        private static Query Delete(IDataDescriptor descriptor, object instance)
        {
            var accessor = ReadAccessor.Create(instance);
            var whereHash = accessor.AsReadOnlyDictionary(instance);
            return Delete(descriptor, whereHash);
        }

        private static Query Delete(IDataDescriptor descriptor, IReadOnlyDictionary<string, object> whereHash)
        {
            var whereHashKeyRewrite = whereHash.Keys.ToDictionary(k => Dialect.ResolveColumnName(descriptor, k), v => v);

            var whereFilter = Dialect.ResolveColumnNames(descriptor).Intersect(whereHashKeyRewrite.Keys).ToList();
            var whereParams = whereFilter.ToDictionary(key => $"{whereHashKeyRewrite[key]}", key => whereHash[whereHashKeyRewrite[key]]);
            var whereParameters = whereParams.Keys.ToList();

            var sql = Dialect.Delete(descriptor, Dialect.ResolveTableName(descriptor), descriptor.Schema, whereFilter, whereParameters);
            var parameters = whereParams.ToDictionary(k => $"{Dialect.Parameter}{k.Key}", v => v.Value);
            return new Query(sql, parameters);
        }
    }
}
