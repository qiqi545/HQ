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
using System.Data;
using System.Diagnostics;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace HQ.Integration.Sqlite.Sql
{
    public static class SqliteConfigurationHelper
    {
        public static void MigrateToLatest(string dataFilePath, IConfiguration configSeed = null, SeedStrategy strategy = SeedStrategy.InsertIfNotExists)
        {
            try
            {
                using (var db = new SqliteConnection($"Data Source={dataFilePath}"))
                {
                    db.Open();

                    MigrateUp(db);

                    if (configSeed != null)
                    {
                        db.SeedInTransaction(configSeed, strategy);
                    }
                }
            }
            catch (SqliteException e)
            {
                Trace.TraceError("Error migrating configuration database: {0}", e);
                throw;
            }
        }

        public static bool IsEmptyConfiguration(string dataFilePath)
        {
            if (!File.Exists(dataFilePath))
                return true;

            try
            {
                using (var db = new SqliteConnection($"Data Source={dataFilePath}"))
                {
                    db.Open();

                    MigrateUp(db);

                    var count = db.ExecuteScalar<int>("SELECT COUNT(1) FROM 'Configuration'");

                    return count == 0;
                }
            }
            catch (SqliteException e)
            {
                Trace.TraceError("Error migrating configuration database: {0}", e);
                throw;
            }
        }

        private static void MigrateUp(IDbConnection db)
        {
            db.Execute(@"
CREATE TABLE IF NOT EXISTS 'Configuration'
(  
    'Key' VARCHAR(128) NOT NULL,
    'Value' VARCHAR(255) NOT NULL,
    UNIQUE(Key)
);");
        }

        public static void SeedInTransaction(this SqliteConnection db, IConfiguration configSeed, SeedStrategy strategy = SeedStrategy.InsertIfNotExists)
        {
            var t = db.BeginTransaction();

            switch (strategy)
            {
                case SeedStrategy.InsertIfNotExists:
                    InsertIfNotExists();
                    break;
                case SeedStrategy.InsertIfEmpty:
                    var count = db.ExecuteScalar<int>("SELECT COUNT(1) FROM 'Configuration'");
                    if (count == 0)
                        InsertIfNotExists();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            t.Commit();

            void InsertIfNotExists()
            {
                foreach (var entry in configSeed.AsEnumerable())
                {
                    db.Execute("INSERT OR IGNORE INTO 'Configuration' ('Key', 'Value') VALUES (@Key, @Value)",
                        new {entry.Key, entry.Value}, t);
                }
            }
        }
    }
}
