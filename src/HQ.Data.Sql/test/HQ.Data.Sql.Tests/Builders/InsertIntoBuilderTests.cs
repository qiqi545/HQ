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
using HQ.Data.Sql.Builders;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.SqlServer;
using Xunit;

namespace HQ.Data.Sql.Tests.Builders
{
    public class InsertIntoBuilderTests
    {
        [Fact]
        public void Insert_into_no_dialect()
        {
            var sql = NoDialect.Default.InsertInto("Foo", null, new List<string> {"CreatedAt"}, false);
            Assert.Equal("INSERT INTO Foo (CreatedAt) VALUES (@CreatedAt)", sql);
        }

        [Fact]
        public void Insert_into_SQL_Server_with_return_keys()
        {
            var d = new SqlServerDialect();
            var sql = d.InsertInto("Foo", "dbo", new List<string> {"CreatedAt"}, true);
            Assert.Equal("INSERT INTO [dbo].[Foo] ([CreatedAt]) OUTPUT Inserted.Id VALUES (@CreatedAt)", sql);
        }
    }
}
