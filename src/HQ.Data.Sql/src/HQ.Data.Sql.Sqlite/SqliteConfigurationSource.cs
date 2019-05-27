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

using System.Diagnostics;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace HQ.Data.Sql.Sqlite
{
    public class SqliteConfigurationSource : IConfigurationSource
    {
        public SqliteConfigurationSource(string dataFilePath)
        {
            DataFilePath = dataFilePath;
            DataDirectoryPath = new FileInfo(DataFilePath).Directory?.FullName;
            DataFileName = Path.GetFileName(DataFilePath);
        }

        public string DataFilePath { get; }
        public string DataDirectoryPath { get; }
        public string DataFileName { get; }
        public bool ReloadOnChange { get; set; }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            if (DataDirectoryPath != null)
                Directory.CreateDirectory(DataDirectoryPath);

            try
            {
                using (var db = new SqliteConnection($"Data Source={DataFilePath}"))
                {
                    db.Execute(@"
CREATE TABLE IF NOT EXISTS 'Configuration'
(  
    'Key' VARCHAR(128) NOT NULL,
    'Value' VARCHAR(255) NOT NULL
);");
                }
            }
            catch (SqliteException e)
            {
                Trace.TraceError("Error migrating configuration database", e);
                throw;
            }

            return new SqliteConfigurationProvider(this);
        }
    }
}
