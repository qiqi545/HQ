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

using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.Queries;
using HQ.Data.Sql.Tests.Models;
using Xunit;
using Xunit.Abstractions;

namespace HQ.Data.Sql.Tests.Queries
{
    public class UpdateTests
    {
        public UpdateTests(ITestOutputHelper console)
        {
            _console = console;
        }

        private readonly ITestOutputHelper _console;

        [Fact]
        public void Bad_set_clause_is_filtered_out()
        {
            var descriptor = SimpleDataDescriptor.Create<User>();
            var query = SqlBuilder.Update(descriptor, set: new {Email = "good@email.com", Foo = "Bar"});
            Assert.Equal("UPDATE User SET Email = @Email_set", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal("good@email.com", query.Parameters["@Email_set"]);
            _console.WriteLine(query.ToString());
        }

        [Fact]
        public void Update_with_explicit_where()
        {
            var query = SqlBuilder.Update<User>(new {Email = "good@email.com"}, new {Email = "bad@email.com"});
            Assert.Equal("UPDATE User SET Email = @Email_set WHERE Email = @Email", query.Sql);
            Assert.Equal(2, query.Parameters.Count);
            Assert.Equal("bad@email.com", query.Parameters["@Email"]);
            Assert.Equal("good@email.com", query.Parameters["@Email_set"]);
            _console.WriteLine(query.ToString());
        }

        [Fact]
        public void Update_with_instance()
        {
            var user = new User {Id = 1, Email = "good@email.com"};
            var query = SqlBuilder.Update(user);
            Assert.Equal("UPDATE User SET Email = @Email_set WHERE Id = @Id", query.Sql);
            Assert.Equal(2, query.Parameters.Count);
            Assert.Equal(1, query.Parameters["@Id"]);
            Assert.Equal("good@email.com", query.Parameters["@Email_set"]);
            _console.WriteLine(query.ToString());
        }

        [Fact]
        public void Update_with_no_where()
        {
            var query = SqlBuilder.Update<User>(new {Email = "good@email.com"});
            Assert.Equal("UPDATE User SET Email = @Email_set", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal("good@email.com", query.Parameters["@Email_set"]);
            _console.WriteLine(query.ToString());
        }
    }
}
