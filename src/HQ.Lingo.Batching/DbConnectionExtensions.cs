using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using HQ.Lingo.Descriptor;

namespace HQ.Lingo.Batching
{
    public static class DbConnectionExtensions
    {
        public static Task CopyAsync<T, TOptions>(this IDbConnection connection, IDataBatchOperation<T> batch, IEnumerable<T> stream, long startingAt = 0, int? count = null, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return batch.ExecuteAsync(connection, SimpleDataDescriptor.Create<T>(), stream, startingAt, count, transaction, commandTimeout);
        }

        public static Task CopyAsync<T, TOptions>(this IDbConnection connection, IDataBatchOperation<T> batch, IDataDescriptor descriptor, IEnumerable<T> stream, long startingAt = 0, int? count = null, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return batch.ExecuteAsync(connection, descriptor, stream, startingAt, count, transaction, commandTimeout);
        }

        public static Task UpdateAsync<T, TOptions>(this IDbConnection connection, IDataBatchOperation<T> batch, IEnumerable<T> stream, long startingAt = 0, int? count = null, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return batch.ExecuteAsync(connection, SimpleDataDescriptor.Create<T>(), stream, startingAt, count, transaction, commandTimeout);
        }

        public static Task UpdateAsync<T, TOptions>(this IDbConnection connection, IDataBatchOperation<T> batch, IDataDescriptor descriptor, IEnumerable<T> stream, long startingAt = 0, int? count = null, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return batch.ExecuteAsync(connection, descriptor, stream, startingAt, count, transaction, commandTimeout);
        }
    }
}
