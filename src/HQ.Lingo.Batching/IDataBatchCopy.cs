using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using HQ.Lingo.Descriptor;

namespace HQ.Lingo.Batching
{
    public interface IDataBatchOperation<TOptions>
    {
        TOptions Before(IDbConnection connection, IDataDescriptor descriptor, IDbTransaction transaction = null, int? commandTimeout = null);
        void Execute<T>(IDbConnection connection, IDataDescriptor descriptor, IEnumerable<T> objects, long? startingAt = 0, int? count = null, IDbTransaction transaction = null, int? commandTimeout = null);
        Task ExecuteAsync<T>(IDbConnection connection, IDataDescriptor descriptor, IEnumerable<T> objects, long? startingAt = 0, int? count = null, IDbTransaction transaction = null, int? commandTimeout = null);
        void After<T>(IDbConnection connection, IDataDescriptor descriptor, TOptions options, IDbTransaction transaction = null, int? commandTimeout = null);
    }
}
