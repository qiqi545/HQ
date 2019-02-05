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
using HQ.Data.Sql.Dialects;
using HQ.PetaPoco.Utilities;

namespace HQ.Data.Sql.MySql
{
    public class MySqlDialect : SqlDialect
    {
        public override char? StartIdentifier => '`';
        public override char? EndIdentifier => '`';
        public override char? Separator => '.';
        public override char? Parameter => '@';
        public override char? Quote => '\'';

        public override bool TryFetchInsertedKey(FetchInsertedKeyLocation location, out string sql)
        {
            switch (location)
            {
                case FetchInsertedKeyLocation.AfterStatement:
                    sql = "SELECT LAST_INSERT_ID() AS `Id`";
                    return true;
                default:
                    sql = null;
                    return false;
            }
        }

        public override void Page(string sql, StringBuilder sb)
        {
            // TODO: similar efficiency considerations for large LIMITs as SQLite
            // https://stackoverflow.com/questions/3799193/mysql-data-best-way-to-implement-paging

            PagingHelper.SplitSql(sql, out var parts);

            var orderBy = parts.SqlOrderBy ?? " ";

            var selectClause = parts.SqlOrderBy == null
                ? parts.SqlSelectRemoved
                : parts.SqlSelectRemoved.Replace(parts.SqlOrderBy, string.Empty);

            sb.Append("SELECT ").Append(selectClause).Append(orderBy).Append(" LIMIT @PerPage OFFSET @Page");
        }
    }
}
