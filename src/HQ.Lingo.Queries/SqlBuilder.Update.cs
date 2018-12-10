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
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using HQ.DotLiquid;
using HQ.Lingo.Builders;
using HQ.Lingo.Descriptor;
using HQ.Lingo.Dialects;

namespace HQ.Lingo.Queries
{
    // TODO no ToList/ToArray via StringBank
    // TODO remove hashKeysRewrite

    partial class SqlBuilder
    {
        public static Query Update<T>(T instance, dynamic where = null)
        {
            var descriptor = GetDescriptor<T>();
            var set = Dialect.ResolveColumnNames(descriptor, ColumnScope.Updated).ToList();

            IDictionary<string, object> hash = Hash.FromAnonymousObject(instance, true);
            var hashKeysRewrite = hash.Keys.ToDictionary(k => Dialect.ResolveColumnName(descriptor, k), v => v);

            IDictionary<string, object> whereHash;
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
                whereHash = Hash.FromAnonymousObject(where, true);
                var whereHashKeysRewrite =
                    whereHash.Keys.ToDictionary(k => Dialect.ResolveColumnName(descriptor, k), v => v);
                whereFilter = Dialect.ResolveColumnNames(descriptor).Intersect(whereHashKeysRewrite.Keys).ToList();
            }

            var setFilter = set.Intersect(hashKeysRewrite.Keys).ToList();
            return Update(descriptor, setFilter, whereFilter, hash, whereHash);
        }

        public static Query Update<T>(dynamic set, dynamic where = null)
        {
            return Update(GetDescriptor<T>(), set, where);
        }

        public static Query Update(object instance)
        {
            return Update(GetDescriptor(instance.GetType()), instance);
        }

        public static Query Update(IDataDescriptor descriptor, object instance)
        {
            IDictionary<string, object> hash = Hash.FromAnonymousObject(instance, true);
            var setFilter = descriptor.Updated.Select(c => c.ColumnName).Intersect(hash.Keys).ToList();
            var whereFilter = descriptor.Keys.Select(c => c.ColumnName).Intersect(hash.Keys).ToList();

            return Update(descriptor, setFilter, whereFilter, hash, hash);
        }

        public static Query Update(IDataDescriptor descriptor, dynamic set, dynamic where = null)
        {
            IDictionary<string, object> setHash = Hash.FromAnonymousObject(set, true);
            var setFilter = descriptor.Updated.Select(c => c.ColumnName).Intersect(setHash.Keys).ToList();

            IDictionary<string, object> whereHash = Hash.FromAnonymousObject(where, true);
            var whereFilter = Dialect.ResolveColumnNames(descriptor).Intersect(whereHash.Keys).ToList();

            return Update(descriptor, setFilter, whereFilter, setHash, whereHash);
        }

        public static Query Update<T>(T instance, List<string> fields)
        {
            return Update(instance, GetDescriptor<T>(), fields);
        }

        public static Query Update<T>(T instance, IDataDescriptor descriptor, List<string> fields)
        {
            if (fields == null || fields.Count == 0)
                return Update(instance);

            IDictionary<string, object> set = new ExpandoObject();
            foreach (var updated in descriptor.Updated)
                if (fields.Contains(updated.ColumnName))
                    set.Add(updated.ColumnName, updated.Property.Get(instance));

            return Update(set);
        }

        private static Query Update(IDataDescriptor descriptor, List<string> setFilter, List<string> whereFilter,
            IDictionary<string, object> setHash, IDictionary<string, object> whereHash)
        {
            var setHashKeyRewrite = setHash.Keys.ToDictionary(k => Dialect.ResolveColumnName(descriptor, k), v => v);
            var whereHashKeyRewrite = RuntimeHelpers.Equals(setHash, whereHash)
                ? setHashKeyRewrite
                : whereHash.Keys.ToDictionary(k => Dialect.ResolveColumnName(descriptor, k), v => v);

            var whereParams = whereFilter.ToDictionary(key => whereHashKeyRewrite[key],
                key => whereHash[whereHashKeyRewrite[key]]);

            var setParams = setFilter.ToDictionary(key => setHashKeyRewrite[key],
                key => setHash[setHashKeyRewrite[key]]);

            var setParameters = setParams.Keys.ToList();
            var whereParameters = whereParams.Keys.ToList();

            var sql = Dialect.Update(descriptor, Dialect.ResolveTableName(descriptor), descriptor.Schema, setFilter,
                whereFilter, setParameters, whereParameters, Dialect.SetSuffix);

            var parameters = Hash.FromDictionary(setParams.ToDictionary(k => k.Key + Dialect.SetSuffix, v => v.Value));
            parameters.Merge(whereParams);

            return new Query(sql, parameters);
        }
    }
}
