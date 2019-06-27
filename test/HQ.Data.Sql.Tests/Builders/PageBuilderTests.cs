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

using HQ.Data.Sql.Builders;
using Xunit;
using Xunit.Abstractions;

namespace HQ.Data.Sql.Tests.Builders
{
    public class PageBuilderTests : SqlBuilderTests
    {
        public PageBuilderTests(ITestOutputHelper console) : base(console)
        {
        }

        [Theory]
        [InlineData(DatabaseType.Sqlite,
            "SELECT * FROM Customer ORDER BY FirstName",
            "SELECT * FROM Customer WHERE FirstName > :LastFirstName ORDER BY FirstName LIMIT :PerPage")]
        [InlineData(DatabaseType.SqlServer,
            "SELECT * FROM Customer",
            ";WITH pages AS ( SELECT Id FROM Customer ORDER BY [Id] OFFSET @PerPage * (@Page - 1) ROWS FETCH NEXT @PerPage ROWS ONLY ) SELECT * FROM Customer WHERE EXISTS (SELECT 1 FROM pages WHERE pages.Id = r.Id)")]
        public void Can_page_with_expected_results(DatabaseType t, string q, string e)
        {
            BuildAndCompare(t, e, d => PageBuilder.Page(d, q));
        }
    }
}
