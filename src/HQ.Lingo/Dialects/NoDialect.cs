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
using HQ.Lingo.Extensions;
using HQ.PetaPoco.Utilities;

namespace HQ.Lingo.Dialects
{
    public class NoDialect : SqlDialect
    {
        public static readonly NoDialect Default = new NoDialect();

        public NoDialect(char? startIdentifier = null, char? endIdentifier = null, char? separator = null,
            char? parameter = '@', char? quote = '\'')
        {
            StartIdentifier = startIdentifier;
            EndIdentifier = endIdentifier;
            Separator = separator;
            Parameter = parameter;
            Quote = quote;
        }

        public override char? StartIdentifier { get; }
        public override char? EndIdentifier { get; }
        public override char? Separator { get; }
        public override char? Parameter { get; }
        public override char? Quote { get; }

        public override bool TryFetchInsertedKey(FetchInsertedKeyLocation location, out string sql)
        {
            sql = null;
            return false;
        }

        public override void Page(string sql, StringBuilder sb)
        {
            PagingHelper.SplitSql(sql, out var parts);

            var orderBy = parts.SqlOrderBy ?? " ";

            var selectClause = parts.SqlOrderBy == null
                ? parts.SqlSelectRemoved
                : parts.SqlSelectRemoved.Replace(parts.SqlOrderBy, string.Empty);

            sb.Append("SELECT ").Append(selectClause)
                .Append(orderBy)
                .Append(" LIMIT ").AppendParameter(this, "PerPage")
                .Append(" OFFSET ").AppendParameter(this, "Page");
        }
    }
}
