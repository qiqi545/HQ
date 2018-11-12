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
using HQ.Lingo.Dialects;
using Xunit;
using Xunit.Abstractions;

namespace HQ.Lingo.Tests.Builders
{
    public class ProjectionBuilderTests
    {
        public ProjectionBuilderTests(ITestOutputHelper console)
        {
            _console = console;
        }

        private readonly ITestOutputHelper _console;

        [Fact]
        public void One_to_many_through_table()
        {
            IList<Filter> filters = new[] {new Filter {Field = "Email", Value = "NULL"}};
            IList<Projection> projections = new[] {new Projection {Type = ProjectionType.OneToMany, Field = "Orders"}};

            var d = NoDialect.Default;
            var sql = d.Select("Customer", "dbo", new[] {"Id", "Email"}, projections, filters);

            _console.WriteLine(sql);
            Assert.Equal((string) ("SELECT p.Id, p.Email, p0.* " +
                                   "FROM dbo.Customer p " +
                                   "LEFT JOIN CustomerOrder c0 ON c0.CustomerId = p.Id " +
                                   "LEFT JOIN Order p0 ON p0.Id = c0.OrderId"), (string) sql);
        }

        [Fact]
        public void One_to_one()
        {
            IList<Filter> filters = new[] {new Filter {Field = "Email", Value = "NULL"}};
            IList<Projection> projections = new[] {new Projection {Type = ProjectionType.OneToOne, Field = "Address"}};

            var d = NoDialect.Default;
            var sql = d.Select("Customer", "dbo", new[] {"Id", "Email"}, projections, filters);

            _console.WriteLine(sql);
            Assert.Equal((string) ("SELECT p.Id, p.Email, p0.* " +
                                   "FROM dbo.Customer p " +
                                   "LEFT JOIN Address p0 ON p0.Id = p.AddressId"), (string) sql);
        }
    }
}
