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

using HQ.Lingo.Dapper;
using HQ.Lingo.Tests.Fakes;
using HQ.Lingo.Tests.Models;
using Xunit;
using Xunit.Abstractions;

namespace HQ.Lingo.Tests.Dapper
{
    public class InsertTests
    {
        public InsertTests(ITestOutputHelper console)
        {
            _console = console;
        }

        private readonly ITestOutputHelper _console;

        [Fact]
        public void Insert_one()
        {
            var db = new FakeDbConnection();
            db.Insert(new User {Email = "good@email.com"});
            var query = db.GetLastQuery();

            Assert.Equal("INSERT INTO User (Email) VALUES (@Email)", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal("good@email.com", query.Parameters["@Email"]);
            _console.WriteLine(query.Sql);
        }
    }
}
