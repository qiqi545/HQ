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
using HQ.Lingo.Builders;
using HQ.Lingo.Dialects;
using Xunit;

namespace HQ.Lingo.Tests.Builders
{
    public class OrderByBuilderTests
    {
        [Fact]
        public void Multiple_qualfied_clauses()
        {
            var sql = NoDialect.Default.OrderBy(new[]
            {
                new Tuple<string, string, bool>("x", "CreatedAt", true),
                new Tuple<string, string, bool>("y", "CreatedBy", false)
            });
            Assert.Equal("ORDER BY x.CreatedAt DESC, y.CreatedBy ASC", sql);
        }

        [Fact]
        public void Single_unqualified_clause()
        {
            var sql = NoDialect.Default.OrderBy(new[] {new Tuple<string, string, bool>(null, "CreatedAt", true)});
            Assert.Equal("ORDER BY CreatedAt DESC", sql);
        }
    }
}
