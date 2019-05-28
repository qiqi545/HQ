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
using System.Linq.Expressions;
using System.Reflection;
using HQ.Common;
using HQ.Data.Sql.Builders;
using HQ.Data.Sql.Descriptor;
using TypeKitchen;

namespace HQ.Data.Sql.Queries
{
    // TODO no ToList via StringBank
    // TODO remove hashKeysRewrite

    partial class SqlBuilder
    {
        public static Query Count<T>()
        {
            return Count(GetDescriptor<T>(), null);
        }

        public static Query Count<T>(dynamic where)
        {
            return Count(GetDescriptor<T>(), where);
        }

        public static Query Select<T>(params Expression<Func<T, object>>[] orderBy)
        {
            return Select(GetDescriptor<T>(), null, orderBy: orderBy);
        }

        public static Query Select<T>(IDataDescriptor descriptor, params Expression<Func<T, object>>[] orderBy)
        {
            return Select(descriptor, null, orderBy: orderBy);
        }

        public static Query Select<T>(int page, int perPage, params Expression<Func<T, object>>[] orderBy)
        {
            return Select(GetDescriptor<T>(), null, null, page, perPage, orderBy);
        }

        public static Query Select<T>(IDataDescriptor descriptor, int page, int perPage,
            params Expression<Func<T, object>>[] orderBy)
        {
            return Select(descriptor, null, null, page, perPage, orderBy);
        }

        public static Query Select<T>(dynamic where, params Expression<Func<T, object>>[] orderBy)
        {
            Query query = Select(GetDescriptor<T>(), null, where);

            if (orderBy?.Length > 0)
                query.Sql = Dialect.OrderBy(query.Sql, orderBy);

            return query;
        }

        public static Query Select<T>(IDataDescriptor descriptor, dynamic where,
            params Expression<Func<T, object>>[] orderBy)
        {
            Query query = Select(descriptor, columnFilter: null, where: where);

            if (orderBy?.Length > 0)
                query.Sql = Dialect.OrderBy(query.Sql, orderBy);

            return query;
        }

        public static Query Select<T>(dynamic where, int page, int perPage,
            params Expression<Func<T, object>>[] orderBy)
        {
            return Select(GetDescriptor<T>(), null, where, page, perPage, orderBy);
        }

        public static Query Select<T>(List<string> columns, params Expression<Func<T, object>>[] orderBy)
        {
            var descriptor = GetDescriptor<T>();
            var columnFilter = Dialect.ResolveColumnNames(descriptor).Intersect(columns).ToList();

            var query = Select(descriptor, columnFilter, where: null);
            if (orderBy?.Length > 0)
                query.Sql = Dialect.OrderBy(query.Sql, orderBy);

            return query;
        }

        public static Query Select<T>(List<string> columns, int page, int perPage,
            params Expression<Func<T, object>>[] orderBy)
        {
            var descriptor = GetDescriptor<T>();
            var columnFilter = Dialect.ResolveColumnNames(descriptor).Intersect(columns).ToList();

            return Select(descriptor, columnFilter, null, page, perPage, orderBy);
        }

        public static Query Select<T>(IList<string> columns, dynamic where,
            params Expression<Func<T, object>>[] orderBy)
        {
            var descriptor = GetDescriptor<T>();
            var columnFilter = Dialect.ResolveColumnNames(descriptor).Intersect(columns).ToList();

            Query query = Select(descriptor, columnFilter, where);
            if (orderBy?.Length > 0)
                query.Sql = Dialect.OrderBy(query.Sql, orderBy);

            return query;
        }

        public static Query Select<T>(IList<string> columns, dynamic where, int page, int perPage,
            params Expression<Func<T, object>>[] orderBy)
        {
            var descriptor = GetDescriptor<T>();
            var columnFilter = Dialect.ResolveColumnNames(descriptor).Intersect(columns).ToArray();

            return Select(descriptor, columnFilter, where, page, perPage, orderBy);
        }

        public static Query Select<T>(T example, params Expression<Func<T, object>>[] orderBy)
        {
            var type = example.GetType();
            var descriptor = GetDescriptor(type); // not always T, because T can be a contract for shaping!
            var columns = GetColumnShape<T>(type, descriptor);

            var query = Select(descriptor, columns, where: null);
            if (orderBy?.Length > 0)
                query.Sql = Dialect.OrderBy(query.Sql, orderBy);
            return query;
        }

        public static Query Select<T>(T example, int page, int perPage, params Expression<Func<T, object>>[] orderBy)
        {
            var type = example.GetType();
            var descriptor = GetDescriptor(type); // not always T, because T can be a contract for shaping!
            var columns = GetColumnShape<T>(type, descriptor);

            return Select(descriptor, columns, null, page, perPage, orderBy);
        }

        public static Query Select<T>(T example, dynamic where, params Expression<Func<T, object>>[] orderBy)
        {
            var type = example.GetType();
            var descriptor = GetDescriptor(type); // not always T, because T can be a contract for shaping!
            var columns = GetColumnShape<T>(type, descriptor);

            Query query = Select(descriptor, columnFilter: columns, where: where);
            if (orderBy?.Length > 0)
                query.Sql = Dialect.OrderBy(query.Sql, orderBy);
            return query;
        }

        public static Query Select<T>(T example, dynamic where, int page, int perPage,
            params Expression<Func<T, object>>[] orderBy)
        {
            var type = example.GetType();
            var descriptor = GetDescriptor(type); // not always T, because T can be a contract for shaping!
            var columns = GetColumnShape<T>(type, descriptor);

            return Select(descriptor, columnFilter: columns, where: where, page: page, perPage: perPage,
                orderBy: orderBy);
        }

        private static List<string> GetColumnShape<T>(Type type, IDataDescriptor descriptor)
        {
            List<string> columns;
            var columnNames = Dialect.ResolveColumnNames(descriptor);
            if (typeof(T) == type)
            {
                // concrete example, we can assume the columns match the descriptor
                columns = columnNames.ToList();
            }
            else
            {
                if (!typeof(T).IsInterface)
                {
                    // base class hierarchy where we can simply carve out the intersection from existing flattened properties
                    var contract = typeof(T)
                        .GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                        .Select(x => x.Name);

                    columns = contract.Intersect(columnNames).OrderBy(x => x).ToList();
                }
                else
                {
                    // no such thing as interface inheritance, which means we can get a flat union easily
                    var contract = new[] {typeof(T)}.Concat(typeof(T).GetInterfaces())
                        .SelectMany(i => i.GetProperties())
                        .Select(x => x.Name);

                    columns = contract.Intersect(columnNames).OrderBy(x => x).ToList();
                }
            }

            return columns;
        }

        private static Query Select(IDataDescriptor descriptor, List<string> columnFilter, dynamic where)
        {
            QueryAndParameters qp = BuildSelectQueryAndParameters(descriptor, columnFilter, where);

            return new Query(qp.sql, qp.parameters);
        }

        private static Query Count(IDataDescriptor descriptor, dynamic where)
        {
            QueryAndParameters qp = BuildSelectQueryAndParameters(descriptor, new List<string> { Dialect.Count }, where);

            return new Query(qp.sql, qp.parameters);
        }

        private static Query Select<T>(IDataDescriptor descriptor, List<string> columnFilter, dynamic where, int page,
            int perPage, params Expression<Func<T, object>>[] orderBy)
        {
            QueryAndParameters qp = BuildSelectQueryAndParameters(descriptor, columnFilter, where);

            if (orderBy?.Length > 0)
                qp.sql = Dialect.OrderBy(qp.sql, orderBy);

            var pageSql = StringBuilderPool.Scoped(sb => { Dialect.Page(qp.sql, sb); });

            qp.parameters.Add($"{Dialect.Parameter}Page", page);
            qp.parameters.Add($"{Dialect.Parameter}PerPage", perPage);

            return new Query(pageSql, qp.parameters);
        }
        
        private static QueryAndParameters BuildSelectQueryAndParameters(IDataDescriptor descriptor,
            List<string> columnFilter, dynamic where)
        {
            object instance = where ?? new { };
            var accessor = ReadAccessor.Create(instance.GetType());

            var whereHash = accessor.AsReadOnlyDictionary(instance);
            var hashKeysRewrite = whereHash.Keys.ToDictionary(k => Dialect.ResolveColumnName(descriptor, k), v => v);

            var tableName = Dialect.ResolveTableName(descriptor);
            var columnNames = Dialect.ResolveColumnNames(descriptor).ToList();

            var whereFilter = columnNames.Intersect(hashKeysRewrite.Keys).ToList();
            var parameters = whereFilter.ToDictionary(key => $"{hashKeysRewrite[key]}", key => whereHash[hashKeysRewrite[key]]);
            var parameterKeys = parameters.Keys.ToList();

            var columns = Dialect.SupportsSelectStar ? new List<string> {"*"} : columnNames;
            var sql = Dialect.Select(descriptor, tableName, descriptor.Schema,
                columnFilter ?? columns, whereFilter,
                parameterKeys);

            return new QueryAndParameters(sql, parameters);
        }

        private struct QueryAndParameters
        {
            public string sql;
            public readonly Dictionary<string, object> parameters;

            public QueryAndParameters(string sql, Dictionary<string, object> parameters)
            {
                this.sql = sql;
                this.parameters = parameters;
            }
        }
    }
}
