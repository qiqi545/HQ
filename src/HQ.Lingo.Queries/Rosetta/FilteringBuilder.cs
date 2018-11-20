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
using System.Linq;
using HQ.Common.Helpers;
using HQ.Lingo.Dialects;
using HQ.Rosetta;

namespace HQ.Lingo.Queries.Rosetta
{
    public static class FilteringBuilder
    {
        public static string Where(ISqlDialect dialect, FilterOptions filter)
        {
            var clauses = string.Join(" AND ", filter.Fields.Select(f => BuildFilterClause(dialect, f)));

            return $"WHERE {clauses}";
        }

        private static string BuildFilterClause(ISqlDialect dialect, Filter filter)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                sb.Append($"{dialect.StartIdentifier}{filter.Field}{dialect.EndIdentifier}");
                switch (filter.Operator)
                {
                    case FilterOperator.Equal:
                        sb.Append(" = ");
                        break;
                    case FilterOperator.NotEqual:
                        throw new NotImplementedException();
                    default:
                        throw new NotSupportedException();
                }

                sb.Append(filter.Value);
            });
        }
    }
}
