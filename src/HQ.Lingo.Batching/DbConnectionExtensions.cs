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
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using HQ.Lingo.Descriptor;
using HQ.Rosetta;

namespace HQ.Lingo.Batching
{
    public static class DbConnectionExtensions
    {
        public static async Task CopyAsync<T, TOptions>(this IDbConnection connection, IDataBatchOperation<TOptions> batch,
            IEnumerable<T> stream, BatchSaveStrategy saveStrategy = BatchSaveStrategy.Insert, long startingAt = 0, int? count = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            await connection.CopyAsync(batch, SimpleDataDescriptor.Create<T>(), stream, saveStrategy, startingAt, count, transaction, commandTimeout, cancellationToken);
        }

        public static async Task CopyAsync<T, TOptions>(this IDbConnection connection, IDataBatchOperation<TOptions> batch,
            IDataDescriptor descriptor, IEnumerable<T> stream, BatchSaveStrategy saveStrategy = BatchSaveStrategy.Insert, long startingAt = 0, int? count = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var before = await batch.BeforeAsync(connection, descriptor, transaction);
            await batch.ExecuteAsync(connection, descriptor, before.Item1, before.Item2, saveStrategy, stream, startingAt, count, transaction, commandTimeout, cancellationToken);
            await batch.AfterAsync(connection, descriptor, before.Item1, before.Item2, saveStrategy, transaction, commandTimeout);
        }
    }
}

