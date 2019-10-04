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
using HQ.Data.Contracts;
using HQ.Data.Sql.Dialects;

namespace HQ.Data.Sql.Queries
{
	public static class SortingBuilder
	{
		public static readonly Lazy<SortOptions> SortByIdAscending = new Lazy<SortOptions>(DefaultSort);

		public static string OrderBy(ISqlDialect dialect, SortOptions options)
		{
			var clauses = string.Join(", ",
				options.Fields.Select(a =>
					$"r.{dialect.StartIdentifier}{a.Field}{dialect.EndIdentifier} {(a.Descending ? "DESC" : "ASC")}"));

			return $"ORDER BY {clauses}";
		}

		private static SortOptions DefaultSort()
		{
			var sort = new SortOptions();
			sort.Fields.Add(new Sort {Field = "Id", Descending = false});
			return sort;
		}
	}
}