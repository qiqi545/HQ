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
using HQ.Lingo.Dapper;
using HQ.Lingo.Queries;
using HQ.Lingo.Tests.Fakes;
using HQ.Lingo.Tests.Models;
using HQ.Rosetta;
using Xunit;
using Xunit.Abstractions;

namespace HQ.Lingo.Tests.Dapper
{
    public class JoinTests
    {
        public JoinTests(ITestOutputHelper console)
        {
            _console = console;
        }

        private readonly ITestOutputHelper _console;

        [Fact]
        public void Get_by_id()
        {
            var db = new FakeDbConnection();

            db.Query<User, Account, Role, int>(p => p.Id, (p, c) => p.Account = c, p => p.Roles, data: new {Id = 2},
                filters: new List<Filter>
                {
                    new Filter
                    {
                        Type = FilterType.Scalar,
                        Field = "Id",
                        Operator = FilterOperator.Equal,
                        Value = "@Id"
                    }
                },
                projections: new List<Projection>
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
                }, orderBy: x => x.CreatedAt.Desc());

            var query = db.GetLastQuery();
            _console.WriteLine(query.Sql);
        }
    }
}
