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
using HQ.Data.Sql.Dialects;
using HQ.Data.Contracts;
using Xunit;

namespace HQ.Data.Sql.Tests.Builders
{
    public class FilterBuilderTests
    {
        [Fact]
        public void Compound()
        {
            var sql = NoDialect.Default.Where
            (
                new Filter {Type = FilterType.Join, Field = "Id", Operator = FilterOperator.NotEqual, Value = 1},
                new Filter {Type = FilterType.Join, Field = "Email", Operator = FilterOperator.NotEqual, Value = "NULL"}
            );
            Assert.Equal("WHERE c0.Id <> 1 AND c0.Email IS NOT NULL", sql);
        }

        [Fact]
        public void Is_join_not_null()
        {
            var sql = NoDialect.Default.Where(new Filter
                {Type = FilterType.Join, Field = "Email", Operator = FilterOperator.NotEqual, Value = "NULL"});
            Assert.Equal("WHERE c0.Email IS NOT NULL", sql);
        }

        [Fact]
        public void Is_join_null()
        {
            var sql = NoDialect.Default.Where(new Filter {Type = FilterType.Join, Field = "Email", Value = "NULL"});
            Assert.Equal("WHERE c0.Email IS NULL", sql);
        }

        [Fact]
        public void Is_join_scalar()
        {
            var sql = NoDialect.Default.Where(new Filter {Type = FilterType.Join, Field = "Id", Value = 1});
            Assert.Equal("WHERE c0.Id = 1", sql);
        }

        [Fact]
        public void Is_not_join_scalar()
        {
            var sql = NoDialect.Default.Where(new Filter
                {Type = FilterType.Join, Field = "Id", Operator = FilterOperator.NotEqual, Value = 1});
            Assert.Equal("WHERE c0.Id <> 1", sql);
        }

        [Fact]
        public void Is_not_join_scalar_parameter()
        {
            var sql = NoDialect.Default.Where(new Filter
                {Type = FilterType.Join, Field = "Id", Operator = FilterOperator.NotEqual, Value = "@Id"});
            Assert.Equal("WHERE c0.Id <> @Id", sql);
        }

        [Fact]
        public void Is_not_null()
        {
            var sql = NoDialect.Default.Where(new Filter
                {Field = "Email", Operator = FilterOperator.NotEqual, Value = "NULL"});
            Assert.Equal("WHERE p.Email IS NOT NULL", sql);
        }

        [Fact]
        public void Is_not_scalar()
        {
            var sql = NoDialect.Default.Where(new Filter {Field = "Id", Operator = FilterOperator.NotEqual, Value = 1});
            Assert.Equal("WHERE p.Id <> 1", sql);
        }

        [Fact]
        public void Is_null()
        {
            var sql = NoDialect.Default.Where(new Filter {Field = "Email", Value = "NULL"});
            Assert.Equal("WHERE p.Email IS NULL", sql);
        }

        [Fact]
        public void Is_scalar()
        {
            var sql = NoDialect.Default.Where(new Filter {Field = "Id", Value = 1});
            Assert.Equal("WHERE p.Id = 1", sql);
        }
    }
}
