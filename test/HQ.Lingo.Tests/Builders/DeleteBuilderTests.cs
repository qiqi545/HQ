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
using HQ.Lingo.Builders;
using HQ.Lingo.Dialects;
using HQ.Lingo.SqlServer;
using Xunit;

namespace HQ.Lingo.Tests.Builders
{
    public class DeleteBuilderTests
    {
        [Fact]
        public void Delete_no_dialect()
        {
            var d = NoDialect.Default;
            var sql = d.Delete("Foo", null, new List<string>());
            Assert.Equal("DELETE FROM Foo", sql);
        }

        [Fact]
        public void Delete_SQL_Server()
        {
            var d = new SqlServerDialect();
            var sql = d.Delete("Foo", "dbo", new List<string> {"Id", "Key"});
            Assert.Equal("DELETE FROM [dbo].[Foo] WHERE [Id] = @Id AND [Key] = @Key", sql);
        }
    }
}
