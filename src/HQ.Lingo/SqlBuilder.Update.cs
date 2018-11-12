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
using HQ.Lingo.Descriptor;

namespace HQ.Lingo
{
    partial class SqlBuilder
    {
        public const string SetSuffix = "_set";

        public static Query Update<T>(T entity)
        {
            var descriptor = GetDescriptor<T>();
            var hash = DynamicToHash(entity);
            var setClause = BuildSafeSetClause(descriptor, hash);

            string sql;
            var parameters = UpdateSetClause(setClause, descriptor, out sql);

            Dictionary<string, object> keys;
            if (descriptor.Identity != null)
                keys = new Dictionary<string, object>
                {
                    {descriptor.Identity.ColumnName, descriptor.Identity.Property.Get(entity)}
                };
            else
                keys = descriptor.Keys.ToDictionary(id => id.ColumnName, id => id.Property.Get(entity));
            var whereClause = WhereClauseByExample(descriptor, keys);

            sql = string.Concat(sql, " WHERE ", whereClause.Sql);
            return new Query(sql, parameters.AddRange(whereClause.Parameters));
        }

        private static Dictionary<string, object> BuildSafeSetClause(IDataDescriptor descriptor,
            IDictionary<string, object> hash)
        {
            var setClause = new Dictionary<string, object>();
            foreach (var insertable in descriptor.Inserted)
            {
                object value;
                if (hash.TryGetValue(insertable.ColumnName, out value)) setClause.Add(insertable.ColumnName, value);
            }

            return setClause;
        }

        public static Query Update<T>(dynamic set, dynamic where = null)
        {
            var descriptor = GetDescriptor<T>();
            var setClause = (IDictionary<string, object>) BuildSafeSetClause(descriptor, DynamicToHash(set));

            string sql;
            var parameters = UpdateSetClause(setClause, descriptor, out sql);

            if (where == null) return new Query(sql, parameters);

            Query whereClause = WhereClauseByExample(descriptor, where);
            sql = string.Concat(sql, " WHERE ", whereClause.Sql);
            return new Query(sql, parameters.AddRange(whereClause.Parameters));
        }

        private static IDictionary<string, object> UpdateSetClause(IDictionary<string, object> setClause,
            IDataDescriptor descriptor, out string sql)
        {
            var parameters = ParametersFromHash(setClause, suffix: SetSuffix);
            var setColumns = ColumnsFromHash(descriptor, setClause);
            sql = string.Concat("UPDATE ", TableName(descriptor), " SET ",
                ColumnParameterClauses(setColumns, SetSuffix).Concat());
            return parameters;
        }
    }
}
