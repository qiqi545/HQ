using System;
using System.Data.Common;
using System.Linq.Expressions;
using Dapper;

namespace tuxedo.Dapper
{
    partial class TuxedoExtensions
    {
        public static int Update<T>(this DbConnection connection, T entity, DbTransaction transaction = null, int? commandTimeout = null, params Expression<Func<T, object>>[] sortOn) where T : class
        {
            var query = Tuxedo.Update(entity);
            var result = connection.Execute(query.Sql, Prepare(query.Parameters), transaction, commandTimeout);
            return result;
        }

        public static int Update<T>(this DbConnection connection, dynamic set, dynamic where = null, DbTransaction transaction = null, int? commandTimeout = null, params Expression<Func<T, object>>[] sortOn) where T : class
        {
            Query query = Tuxedo.Update<T>(set, where);
            var result = connection.Execute(query.Sql, Prepare(query.Parameters), transaction, commandTimeout);
            return result;
        }
    }
}
