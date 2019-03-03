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
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HQ.Common.FastMember;
using HQ.Data.Contracts;
using HQ.Data.Sql.Batching;
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.SqlServer.Configuration;

namespace HQ.Data.Sql.SqlServer
{
    public class SqlBatchCopy : IDataBatchOperation<SqlServerOptions>
    {
        protected internal const string DefaultSchema = "dbo";

        public async Task<(SqlServerOptions, object)> BeforeAsync(IDbConnection connection, IDataDescriptor descriptor,
            IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            var database = connection.Database;
            var settings = new SqlServerOptions
            {
                PageVerify = (await connection
                    .ExecuteAsync("SELECT page_verify_option_desc FROM sys.databases WHERE [NAME] = @Database",
                        new {Database = database}, transaction, commandTimeout)).ToString(),
                RecoveryModel = (await connection
                    .ExecuteAsync("SELECT recovery_model_desc FROM sys.databases WHERE [NAME] =  @Database",
                        new {Database = database}, transaction, commandTimeout)).ToString()
            };

            connection.Execute("USE master;");
            connection.Execute($"ALTER DATABASE [{database}] SET PAGE_VERIFY NONE;");
            connection.Execute($"ALTER DATABASE [{database}] SET RECOVERY BULK_LOGGED");
            connection.Execute($"USE [{database}]");

            return (settings, null);
        }

        public async Task ExecuteAsync<TData>(IDbConnection connection, IDataDescriptor descriptor,
            SqlServerOptions options,
            object userState, BatchSaveStrategy saveStrategy, IEnumerable<TData> objects, long startingAt = 0,
            int? count = null,
            IDbTransaction transaction = null, int? commandTimeout = null,
            CancellationToken cancellationToken = default)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            var reader = new ObjectReader(typeof(TData), objects,
                descriptor.Inserted.Select(x => x.ColumnName).ToArray());

            var mapping = GenerateBulkCopyMapping(descriptor, reader, connection, transaction, commandTimeout);
            using (var bcp = new SqlBulkCopy((SqlConnection) connection, SqlBulkCopyOptions.TableLock,
                (SqlTransaction) transaction))
            {
                foreach (var column in mapping.DatabaseTableColumns)
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column, column));

                // ReSharper disable once PossibleMultipleEnumeration
                bcp.BatchSize = count ?? objects.Count();
                bcp.DestinationTableName =
                    $"[{descriptor.Schema ?? DefaultSchema}].[{mapping.DataReaderTable.TableName}]";
                bcp.BulkCopyTimeout = commandTimeout.GetValueOrDefault();

                await bcp.WriteToServerAsync(reader, cancellationToken);
            }
        }

        public async Task AfterAsync(IDbConnection connection, IDataDescriptor descriptor, SqlServerOptions options,
            object userState,
            BatchSaveStrategy saveStrategy, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var database = connection.Database;
            await connection.ExecuteAsync("USE master");
            await connection.ExecuteAsync($"ALTER DATABASE [{database}] SET PAGE_VERIFY {options.PageVerify};");
            await connection.ExecuteAsync($"ALTER DATABASE [{database}] SET RECOVERY {options.RecoveryModel}");
            await connection.ExecuteAsync($"USE [{database}]");
        }

        private static BatchMap GenerateBulkCopyMapping(IDataDescriptor descriptor, IDataReader reader,
            IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var schemaTable = reader.GetSchemaTable();
            var schemaTableColumns = (from DataColumn column in schemaTable?.Columns select column.ColumnName).ToList();
            var databaseTableColumns =
                GetDatabaseTableColumns(connection, schemaTable?.TableName, transaction, commandTimeout).ToList();
            var excludedColumns = descriptor.Computed.Select(c => c.ColumnName).ToList();

            databaseTableColumns = databaseTableColumns.Except(excludedColumns).ToList();

            return new BatchMap
            {
                DataReaderTable = schemaTable,
                DatabaseTableColumns = databaseTableColumns,
                SchemaTableColumns = schemaTableColumns
            };
        }

        private static IEnumerable<string> GetDatabaseTableColumns(IDbConnection connection, string tableName,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "sp_Columns";
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = commandTimeout ?? 0;

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@table_name";
                parameter.DbType = DbType.String;
                parameter.Size = 384;
                parameter.Value = tableName;
                command.Parameters.Add(parameter);

                if (connection.State == ConnectionState.Closed) connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read()) yield return (string) reader["column_name"];
                }
            }
        }
    }
}
