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
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace HQ.Data.Sql.Sqlite
{
    public class SqliteConfigurationProvider : ConfigurationProvider
    {
        private readonly SqliteConfigurationSource _source;

        public SqliteConfigurationProvider(SqliteConfigurationSource source)
        {
            _source = source;
        }

        public override void Set(string key, string value)
        {
            using (var db = new SqliteConnection($"Data Source={_source.DataFilePath}"))
            {
                db.Open();
                var t = db.BeginTransaction();
                var count = db.Execute(Update, new {Key = key, Value = value});
                if (count == 0)
                    db.Execute(Insert, new {Key = key, Value = value});
                t.Commit();
            }
            Data[key] = value;
        }

        public override void Load()
        {
            Data.Clear();
            using (var db = new SqliteConnection($"Data Source={_source.DataFilePath}"))
            {
                db.Open();
                var data = db.Query<KeyValuePair<string, string>>(GetAll);
                foreach (var item in data)
                    Data[item.Key] = item.Value;
            }
            if(_source.ReloadOnChange)
                OnReload();
        }

        #region SQL

        private const string GetAll = "SELECT 'Key', 'Value' FROM 'Configuration'";
        private const string Update = "UPDATE 'Configuration' SET 'Value' = @Value WHERE 'Key' = @Key;";
        private const string Insert = "INSERT INTO 'Configuration' ('Key','Value') VALUES (@Key,@Value);";

        #endregion
    }
}
