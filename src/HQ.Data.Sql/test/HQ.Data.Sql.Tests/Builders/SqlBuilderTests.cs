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
using System.Text.RegularExpressions;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.MySql;
using HQ.Data.Sql.Sqlite;
using HQ.Data.Sql.SqlServer;
using Xunit;
using Xunit.Abstractions;

namespace HQ.Data.Sql.Tests.Builders
{
    public class SqlBuilderTests
    {
        public enum DatabaseType
        {
            Sqlite,
            MySql,
            SqlServer
        }

        private readonly ITestOutputHelper _console;

        public SqlBuilderTests(ITestOutputHelper console)
        {
            _console = console;
        }

        public static ISqlDialect GetDialect(DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.Sqlite:
                    return new SqliteDialect();
                case DatabaseType.SqlServer:
                    return new SqlServerDialect();
                case DatabaseType.MySql:
                    return new MySqlDialect();
                default:
                    return NoDialect.Default;
            }
        }

        protected void BuildAndCompare(DatabaseType t, string e, Func<ISqlDialect, string> executor)
        {
            var d = GetDialect(t);
            var a = executor(d);
            Assert.NotNull(a);
            _console.WriteLine(a);
            Assert.Equal(
                Regex.Replace(e, @"\s\s+\t?", " ").Replace("\t", ""),
                Regex.Replace(a, @"\s\s+\t?", " ").Replace("\t", ""));
        }
    }
}
