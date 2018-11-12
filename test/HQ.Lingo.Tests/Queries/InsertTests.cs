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

using HQ.Lingo.Descriptor;
using HQ.Lingo.Queries;
using HQ.Lingo.Tests.Models;
using Xunit;
using Xunit.Abstractions;

namespace HQ.Lingo.Tests.Queries
{
    public class InsertTests
    {
        public InsertTests(ITestOutputHelper console)
        {
            _console = console;
        }

        private readonly ITestOutputHelper _console;

        [Fact]
        public void Bad_value_clause_is_filtered_out()
        {
            var descriptor = SimpleDataDescriptor.Create(typeof(User));
            var dynamic = new {Email = "good@email.com", Foo = "Bar"};
            var query = SqlBuilder.Insert(dynamic, descriptor);
            Assert.Equal("INSERT INTO User (Email) VALUES (@Email)", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal("good@email.com", query.Parameters["@Email"]);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Insert_one_anonymous()
        {
            var descriptor = SimpleDataDescriptor.Create(typeof(User));
            var query = SqlBuilder.Insert(new {Email = "good@email.com"}, descriptor);
            Assert.Equal("INSERT INTO User (Email) VALUES (@Email)", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal("good@email.com", query.Parameters["@Email"]);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Insert_one_dynamic()
        {
            var descriptor = SimpleDataDescriptor.Create(typeof(User));
            var dynamic = new {Email = "good@email.com"};
            var query = SqlBuilder.Insert(dynamic, descriptor);
            Assert.Equal("INSERT INTO User (Email) VALUES (@Email)", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal("good@email.com", query.Parameters["@Email"]);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Insert_one_static()
        {
            var query = SqlBuilder.Insert(new User {Email = "good@email.com"});
            Assert.Equal("INSERT INTO User (Email) VALUES (@Email)", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal("good@email.com", query.Parameters["@Email"]);
            _console.WriteLine(query.Sql);
        }
    }
}
