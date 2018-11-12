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

using System.Text;
using HQ.Common;
using HQ.Lingo.Dialects;
using HQ.Lingo.Helpers;

namespace HQ.Lingo.SqlServer
{
    public class SqlServerDialect : SqlDialect
    {
        public override char? StartIdentifier => '[';
        public override char? EndIdentifier => ']';
        public override char? Separator => '.';
        public override char? Parameter => '@';
        public override char? Quote => '\'';

        public override bool TryFetchInsertedKey(FetchInsertedKeyLocation location, out string sql)
        {
            switch (location)
            {
                case FetchInsertedKeyLocation.BeforeValues:
                    sql = "OUTPUT Inserted.Id";
                    return true;
                default:
                    sql = null;
                    return false;
            }
        }

        public override void Page(string sql, StringBuilder sb)
        {
            // choosing performance strategy based on:
            // https://sqlperformance.com/2015/01/t-sql-queries/pagination-with-offset-fetch

            PagingHelper.SplitSql(sql, out var parts);

            var orderBy = parts.SqlOrderBy ?? "";

            var selectClause = parts.SqlOrderBy == null
                ? parts.SqlSelectRemoved
                : parts.SqlSelectRemoved.Replace(parts.SqlOrderBy, string.Empty);

            sb.Append(@";WITH pages AS ( SELECT Id FROM ")
                .Append(parts.SqlFrom)
                .Append(" ORDER BY ")
                .Append(StartIdentifier).Append("Id").Append(EndIdentifier)
                .Append(" OFFSET @PerPage * (@Page - 1) ROWS FETCH NEXT @PerPage ROWS ONLY ) ");

            sb.Append("SELECT ").Append(selectClause).Append(' ')
                .Append(parts.SqlSelectRemoved.Contains("WHERE") ? "AND" : "WHERE")
                .Append(" EXISTS (SELECT 1 FROM pages WHERE pages.Id = ").Append(Constants.Sql.ParentAlias).Append('.')
                .Append("Id").Append(")")
                .Append(orderBy);
        }
    }
}
