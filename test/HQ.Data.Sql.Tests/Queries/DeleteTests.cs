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
    public class DeleteTests
    {
        public DeleteTests(ITestOutputHelper console)
        {
            _console = console;
        }

        private readonly ITestOutputHelper _console;

        [Fact]
        public void Bad_where_clause_is_filtered_out()
        {
            var descriptor = SimpleDataDescriptor.Create<User>();
            var query = SqlBuilder.Delete(descriptor, new { Email = "good@email.com", Foo = "Bar" });
            Assert.Equal("DELETE FROM User WHERE Email = @Email", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal("good@email.com", query.Parameters["@Email"]);
            _console.WriteLine(query.ToString());
        }

        [Fact]
        public void Delete_all()
        {
            var query = SqlBuilder.DeleteAll<User>();
            Assert.Equal("DELETE FROM User", query.Sql);
        }

        [Fact]
        public void Delete_by_id()
        {
            var query = SqlBuilder.Delete<User>();
            Assert.Equal("DELETE FROM User WHERE Id = @Id", query.Sql);
        }

        [Fact]
        public void Delete_with_anonymous()
        {
            var descriptor = SimpleDataDescriptor.Create<User>();
            var query = SqlBuilder.Delete(descriptor, new {Email = "good@email.com"});
            Assert.Equal("DELETE FROM User WHERE Email = @Email", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal("good@email.com", query.Parameters["@Email"]);
            _console.WriteLine(query.ToString());
        }

        [Fact]
        public void Delete_with_where()
        {
            var query = SqlBuilder.Delete<User>(new {Email = "good@email.com"});
            Assert.Equal("DELETE FROM User WHERE Email = @Email", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal("good@email.com", query.Parameters["@Email"]);
            _console.WriteLine(query.ToString());
        }
    }
}
