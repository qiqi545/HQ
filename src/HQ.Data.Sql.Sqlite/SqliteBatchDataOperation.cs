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
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HQ.Data.Contracts;
using HQ.Data.Sql.Batching;
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.Queries;
using HQ.Data.Sql.Sqlite.Configuration;
using Microsoft.Data.Sqlite;

namespace HQ.Data.Sql.Sqlite
{
    public class SqliteBatchDataOperation : IDataBatchOperation<SqliteOptions>
    {
        public Task<(SqliteOptions, object)> BeforeAsync(IDbConnection connection, IDataDescriptor descriptor, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var settings = new SqliteOptions();
            return Task.FromResult<(SqliteOptions, object)>((settings, null));
        }

        public async Task ExecuteAsync<TData>(IDbConnection connection, IDataDescriptor descriptor, SqliteOptions options, object userState,
            BatchSaveStrategy saveStrategy, IEnumerable<TData> objects, long startingAt = 0, int? count = null,
            IDbTransaction transaction = null, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            if (connection is SqliteConnection db)
            {
                switch (saveStrategy)
                {
                    case BatchSaveStrategy.Insert:
                        if(transaction == null)
                            transaction = db.BeginTransaction();
                        foreach (var @object in objects)
                        {
                            var query = SqlBuilder.Insert(@object, descriptor);
                            await db.ExecuteAsync(query.Sql, query.Parameters, transaction);
                        }
                        break;
                    case BatchSaveStrategy.Upsert:
                    case BatchSaveStrategy.Update:
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(saveStrategy), saveStrategy, null);
                }
            }
        }

        public Task AfterAsync(IDbConnection connection, IDataDescriptor descriptor, SqliteOptions options, object userState,
            BatchSaveStrategy saveStrategy, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Task.CompletedTask;
        }
    }
}
