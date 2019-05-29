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
using System.Diagnostics;
using Dapper;
using HQ.Extensions.Options;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace HQ.Data.Sql.Sqlite
{
    public class SqliteConfigurationProvider : ConfigurationProvider, ISaveConfigurationProvider
    {
        private readonly SqliteConfigurationSource _source;

        public SqliteConfigurationProvider(SqliteConfigurationSource source)
        {
            _source = source;
        }

        public bool HasChildren(string key)
        {
            foreach (var entry in Data)
                if (entry.Key.StartsWith(key, StringComparison.Ordinal))
                    return true;
            return false;
        }

        public bool Save<TOptions>(string key, TOptions instance)
        {
            var map = instance.Unbind(key);

            var changed = false;
            using (var db = new SqliteConnection($"Data Source={_source.DataFilePath}"))
            {
                db.Open();

                var t = db.BeginTransaction();

                foreach (var entry in map)
                {
                    if (!Data.TryGetValue(entry.Key, out var value))
                        continue; // deprecated?

                    var before = value;
                    var after = entry.Value;

                    if (before.Equals(after, StringComparison.Ordinal))
                        continue; // no change

                    var count = db.Execute(Update, new { entry.Key, Value = before }, t);
                    if (count == 0)
                        count = db.Execute(Insert, new { entry.Key, Value = before }, t);
                    if (count > 0)
                        changed = true;
                }

                t.Commit();
            }

            return changed;
        }

        public override void Set(string key, string value)
        {
            if (TryGet(key, out var previousValue) && value == previousValue)
                return;
            using (var db = new SqliteConnection($"Data Source={_source.DataFilePath}"))
            {
                db.Open();
                var t = db.BeginTransaction();
                var count = db.Execute(Update, new {Key = key, Value = value}, t);
                if (count == 0)
                    db.Execute(Insert, new {Key = key, Value = value}, t);
                t.Commit();
            }
            Data[key] = value;
            if (_source.ReloadOnChange)
                OnReload();
        }

        public override void Load()
        {
            var onChange = Data.Count > 0;
            Data.Clear();
            using (var db = new SqliteConnection($"Data Source={_source.DataFilePath}"))
            {
                db.Open();
                var data = db.Query<ConfigurationRow>(GetAll);
                foreach (var item in data)
                {
                    Data[item.Key] = item.Value;
                    onChange = true;
                }
            }
            if(onChange && _source.ReloadOnChange)
                OnReload();
        }
        
        [DebuggerDisplay("{Key} = {Value}")]
        private struct ConfigurationRow
        {
#pragma warning disable 649
            public string Key;
            public string Value;
#pragma warning restore 649
        }

        #region SQL

        private const string GetAll = "SELECT * FROM Configuration";
        private const string Update = "UPDATE Configuration SET Value = :Value WHERE Key = :Key";
        private const string Insert = "INSERT INTO Configuration (Key, Value) VALUES (:Key, :Value)";

        #endregion
    }
}
