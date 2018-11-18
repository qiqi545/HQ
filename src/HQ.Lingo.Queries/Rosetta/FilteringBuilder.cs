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
                sb.Append($"{ dialect.StartIdentifier}{filter.Field}{ dialect.EndIdentifier}");
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
