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
using System.Data;
using System.Linq.Expressions;
using Dapper;
using HQ.Lingo.Queries;

namespace HQ.Lingo.Dapper
{
    partial class DapperExtensions
    {
        public static IEnumerable<T> Query<T>(this IDbConnection connection, IDbTransaction transaction = null,
           bool buffered = true, int? commandTimeout = null, CommandType? commandType = null,
           params Expression<Func<T, object>>[] orderBy) where T : class
        {
            var query = SqlBuilder.Select(orderBy);
            var result = connection.Query<T>(query.Sql, transaction, buffered, commandTimeout, commandType);
            return result;
        }

        public static IEnumerable<T> Query<T>(this IDbConnection connection, int page, int perPage,
            IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, params Expression<Func<T, object>>[] orderBy) where T : class
        {
            var query = SqlBuilder.Select(page, perPage, orderBy);
            var result = connection.Query<T>(query.Sql, transaction, buffered, commandTimeout, commandType);
            return result;
        }

        public static IEnumerable<T> Query<T>(this IDbConnection connection, dynamic where,
            IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, params Expression<Func<T, object>>[] orderBy) where T : class
        {
            Query query = SqlBuilder.Select<T>(where, orderBy: orderBy);
            var result = connection.Query<T>(query.Sql, query.Parameters.Prepare(), transaction, buffered,
                commandTimeout, commandType);
            return result;
        }

        public static IEnumerable<T> Query<T>(this IDbConnection connection, dynamic where, int page, int perPage,
            IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, params Expression<Func<T, object>>[] orderBy) where T : class
        {
            Query query = SqlBuilder.Select<T>(where, page, perPage, orderBy: orderBy);
            var result = connection.Query<T>(query.Sql, query.Parameters.Prepare(), transaction, buffered,
                commandTimeout, commandType);
            return result;
        }

        public static IEnumerable<T> Query<T>(this IDbConnection connection, List<string> columns,
            IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, params Expression<Func<T, object>>[] orderBy) where T : class
        {
            var query = SqlBuilder.Select(columns, orderBy);
            var result = connection.Query<T>(query.Sql, transaction, buffered, commandTimeout, commandType);
            return result;
        }

        public static IEnumerable<T> Query<T>(this IDbConnection connection, List<string> columns, int page,
            int perPage, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, params Expression<Func<T, object>>[] orderBy) where T : class
        {
            var query = SqlBuilder.Select(columns, page, perPage, orderBy);
            var result = connection.Query<T>(query.Sql, transaction, buffered, commandTimeout, commandType);
            return result;
        }

        public static IEnumerable<T> Query<T>(this IDbConnection connection, List<string> columns, dynamic where,
            IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, params Expression<Func<T, object>>[] orderBy) where T : class
        {
            Query query = SqlBuilder.Select(columns, where, orderBy: orderBy);
            var result = connection.Query<T>(query.Sql, query.Parameters.Prepare(), transaction, buffered,
                commandTimeout, commandType);
            return result;
        }

        public static IEnumerable<T> Query<T>(this IDbConnection connection, List<string> columns, dynamic where,
            int page, int perPage, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, params Expression<Func<T, object>>[] orderBy) where T : class
        {
            Query query = SqlBuilder.Select(columns, where, page, perPage, orderBy: orderBy);
            var result = connection.Query<T>(query.Sql, query.Parameters.Prepare(), transaction, buffered,
                commandTimeout, commandType);
            return result;
        }

        public static IEnumerable<T> Query<T>(this IDbConnection connection, T example,
            IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, params Expression<Func<T, object>>[] orderBy) where T : class
        {
            var query = SqlBuilder.Select(example, orderBy);
            var result = connection.Query<T>(query.Sql, transaction, buffered, commandTimeout, commandType);
            return result;
        }

        public static IEnumerable<T> Query<T>(this IDbConnection connection, T example, int page, int perPage,
            IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, params Expression<Func<T, object>>[] orderBy) where T : class
        {
            var query = SqlBuilder.Select(example, page, perPage, orderBy);
            var result = connection.Query<T>(query.Sql, transaction, buffered, commandTimeout, commandType);
            return result;
        }

        public static IEnumerable<T> Query<T>(this IDbConnection connection, T example, dynamic where,
            IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, params Expression<Func<T, object>>[] orderBy) where T : class
        {
            Query query = SqlBuilder.Select<T>(example: example, where: where, orderBy: orderBy);
            var result = connection.Query<T>(query.Sql, transaction, buffered, commandTimeout, commandType);
            return result;
        }

        public static IEnumerable<T> Query<T>(this IDbConnection connection, T example, dynamic where, int page,
            int perPage, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, params Expression<Func<T, object>>[] orderBy) where T : class
        {
            Query query = SqlBuilder.Select<T>(example: example, page: page, perPage: perPage, where: where,
                orderBy: orderBy);
            var result = connection.Query<T>(query.Sql, transaction, buffered, commandTimeout, commandType);
            return result;
        }
    }
}
