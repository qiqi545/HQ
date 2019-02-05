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
using HQ.Data.Sql.Builders;
using HQ.Data.Sql.Dialects;
using HQ.Data.Contracts;
using Xunit;
using Xunit.Abstractions;

namespace HQ.Data.Sql.Tests.Builders
{
    public class QueryBuilderTests
    {
        public QueryBuilderTests(ITestOutputHelper console)
        {
            _console = console;
        }

        private readonly ITestOutputHelper _console;

        [Fact]
        public void Get_by_id_with_projections()
        {
            var d = NoDialect.Default;
            var sql = d.Query("User", "dbo",
                new[] {"Id", "Username", "CreatedAt"},
                new[]
                {
                    new Filter
                    {
                        Type = FilterType.Scalar,
                        Field = "Id",
                        Operator = FilterOperator.Equal,
                        Value = "@Id"
                    }
                }, new[]
                {
                    new Projection
                    {
                        Type = ProjectionType.OneToOne,
                        Field = "Account"
                    },
                    new Projection
                    {
                        Type = ProjectionType.OneToMany,
                        Field = "Roles"
                    }
                }, new[] {new Tuple<string, string, bool>(null, "CreatedAt", true)});

            _console.WriteLine(sql);

            Assert.Equal("SELECT p.Id, p.Username, p.CreatedAt, p0.*, p1.* " +
                         "FROM dbo.User p " +
                         "LEFT JOIN Account p0 ON p0.Id = p.AccountId " +
                         "LEFT JOIN UserRole c1 ON c1.UserId = p.Id " +
                         "LEFT JOIN Role p1 ON p1.Id = c1.RoleId " +
                         "WHERE p.Id = @Id " +
                         "ORDER BY CreatedAt DESC", sql);
        }
    }
}
