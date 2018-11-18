using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HQ.Lingo.Dialects;
using HQ.Rosetta;

namespace HQ.Lingo.Queries.Rosetta
{
	public static class SortingBuilder
	{
		public static string OrderBy(ISqlDialect dialect, SortOptions options)
		{
			var clauses = string.Join(", ", options.Fields.Select(a => $"r.{dialect.StartIdentifier}{a.Field}{dialect.EndIdentifier} {(a.Descending ? "DESC" : "ASC")}"));

			return $"ORDER BY {clauses}";
		}

		public static SortOptions OrderBy<T>(Expression<Func<IEnumerable<T>, IOrderedEnumerable<T>>> expression)
		{
			return SortByIdAscending.Value;

			// http://stackoverflow.com/questions/41244/dynamic-linq-orderby-on-ienumerablet
			throw new NotImplementedException();
		}

		public static readonly Lazy<SortOptions> SortByIdAscending = new Lazy<SortOptions>(DefaultSort);

		private static SortOptions DefaultSort()
		{
			var sort = new SortOptions();
			sort.Fields.Add(new Sort { Field = "Id", Descending = false});
			return sort;
		}
	}
}
