using HQ.Common.Helpers;
using HQ.Lingo.Dialects;
using HQ.Rosetta;

namespace HQ.Lingo.Queries.Rosetta
{
    public static class ObjectQueryBuilder
    {
        public static string Build<T>(this ISqlDialect dialect, SortOptions sort = null, FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projections = null)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                // SELECT * FROM ...
                sb.Append(ProjectionBuilder.Select<T>(dialect, fields, projections));

                // WHERE ...
                if (filter?.Fields.Count > 0)
                {
                    sb.Append($" {FilteringBuilder.Where(dialect, filter)}");
                }

                // ORDER BY ...
                if (sort?.Fields.Count > 0)
                    sb.Append($" {SortingBuilder.OrderBy(dialect, sort)}");
            });
        }

        public static string Count<T>(this ISqlDialect dialect, FilterOptions filter = null)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                // SELECT COUNT(1) FROM ...
                sb.Append($"SELECT COUNT(1) FROM {dialect.StartIdentifier}{typeof(T).Name}{dialect.EndIdentifier}");

                // WHERE ...
                if (filter?.Fields.Count > 0)
                {
                    sb.Append($" {FilteringBuilder.Where(dialect, filter)}");
                }
            });
        }
    }
}
