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

using HQ.Data.Sql.Queries;
using HQ.Data.Sql.Tests.Models;
using Xunit;
using Xunit.Abstractions;

namespace HQ.Data.Sql.Tests.Queries
{
    public class SelectTests
    {
        public SelectTests(ITestOutputHelper console)
        {
            _console = console;
        }

        private readonly ITestOutputHelper _console;

        [Fact]
        public void Bad_select_column_filtered_out()
        {
            var query = SqlBuilder.Select<User>(new[] {"Email", "Foo"});
            Assert.Equal("SELECT Email FROM User", query.Sql);
            Assert.Equal(0, query.Parameters.Count);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Bad_where_column_filtered_out()
        {
            var query = SqlBuilder.Select<User>(new[] {"Email", "Foo"}, new {Foo = "Bar"});
            Assert.Equal("SELECT Email FROM User", query.Sql);
            Assert.Equal(0, query.Parameters.Count);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Select_all_static_no_where_no_order_by()
        {
            var query = SqlBuilder.Select<User>();
            Assert.Equal("SELECT CreatedAt, Email, Id FROM User", query.Sql);
            Assert.Equal(0, query.Parameters.Count);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Select_all_static_no_where_with_order_by_ascending()
        {
            var query = SqlBuilder.Select<User>(orderBy: x => x.Email.Asc());
            Assert.Equal("SELECT CreatedAt, Email, Id FROM User ORDER BY Email ASC", query.Sql);
            Assert.Equal(0, query.Parameters.Count);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Select_all_static_no_where_with_order_by_descending()
        {
            var query = SqlBuilder.Select<User>(orderBy: x => x.Email.Desc());
            Assert.Equal("SELECT CreatedAt, Email, Id FROM User ORDER BY Email DESC", query.Sql);
            Assert.Equal(0, query.Parameters.Count);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Select_all_static_with_where_and_multiple_order_by()
        {
            var query = SqlBuilder.Select<User>(new {Id = 2}, x => x.Id, x => x.Email.Desc());
            Assert.Equal("SELECT CreatedAt, Email, Id FROM User WHERE Id = @Id ORDER BY Id ASC, Email DESC", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal(2, query.Parameters["@Id"]);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Select_all_static_with_where_and_multiple_order_by_and_paging()
        {
            var query = SqlBuilder.Select<User>(new {Id = 2}, 3, 20, x => x.Id, x => x.Email.Desc());
            Assert.Equal("SELECT CreatedAt, Email, Id FROM User WHERE Id = @Id ORDER BY Id ASC, Email DESC LIMIT @PerPage OFFSET @Page", query.Sql);
            Assert.Equal(3, query.Parameters.Count);
            Assert.Equal(2, query.Parameters["@Id"]);
            Assert.Equal(3, query.Parameters["@Page"]);
            Assert.Equal(20, query.Parameters["@PerPage"]);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Select_all_static_with_where_no_order_by()
        {
            var query = SqlBuilder.Select<User>(new {Id = 2});
            Assert.Equal("SELECT CreatedAt, Email, Id FROM User WHERE Id = @Id", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal(2, query.Parameters["@Id"]);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Select_all_static_with_where_with_order_by_descending()
        {
            var query = SqlBuilder.Select<User>(where: new {Id = 2}, orderBy: x => x.Email.Desc());
            Assert.Equal("SELECT CreatedAt, Email, Id FROM User WHERE Id = @Id ORDER BY Email DESC", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal(2, query.Parameters["@Id"]);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Select_subset_by_base_class_contract_example()
        {
            var query = SqlBuilder.Select<EmailEntity>(new User(), 1, 10, x => x.Email.Desc());
            Assert.Equal("SELECT Email FROM User ORDER BY Email DESC LIMIT @PerPage OFFSET @Page", query.Sql);
            Assert.Equal(2, query.Parameters.Count);
            Assert.Equal(1, query.Parameters["@Page"]);
            Assert.Equal(10, query.Parameters["@PerPage"]);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Select_subset_by_compound_contract_example()
        {
            var query = SqlBuilder.Select<IUser>(new User(), new {Id = 1}, 1, 10, x => x.Id);
            Assert.Equal("SELECT Email, Id FROM User WHERE Id = @Id ORDER BY Id ASC LIMIT @PerPage OFFSET @Page", query.Sql);
            Assert.Equal(3, query.Parameters.Count);
            Assert.Equal(1, query.Parameters["@Id"]);
            Assert.Equal(1, query.Parameters["@Page"]);
            Assert.Equal(10, query.Parameters["@PerPage"]);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Select_subset_by_concrete_example()
        {
            var query = SqlBuilder.Select(new User(), new {Id = 1}, 1, 10, x => x.Id);
            Assert.Equal("SELECT CreatedAt, Id, Email FROM User WHERE Id = @Id ORDER BY Id ASC LIMIT @PerPage OFFSET @Page", query.Sql);
            Assert.Equal(3, query.Parameters.Count);
            Assert.Equal(1, query.Parameters["@Id"]);
            Assert.Equal(1, query.Parameters["@Page"]);
            Assert.Equal(10, query.Parameters["@PerPage"]);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Select_subset_by_simple_contract_example()
        {
            var query = SqlBuilder.Select<IHasIdKey>(new User());
            Assert.Equal("SELECT Id FROM User", query.Sql);
            Assert.Equal(0, query.Parameters.Count);
            _console.WriteLine(query.Sql);
        }

        [Fact]
        public void Select_subset_explicit_list()
        {
            var query = SqlBuilder.Select<User>(new[] {"Email"});
            Assert.Equal("SELECT Email FROM User", query.Sql);
            Assert.Equal(0, query.Parameters.Count);
            _console.WriteLine(query.Sql);
        }
    }
}
