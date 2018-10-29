// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace HQ.Rosetta
{
	public class QueryContext
	{
		public Type Type { get; set; }
		public MethodInfo Handle { get; set; }
		public ICollection<Error> Errors { get; } = new List<Error>();

		public FieldOptions Fields { get; set; }
		public SortOptions Sort { get; set; }
		public PageOptions Paging { get; set; }
		public FilterOptions Filters { get; set; }
		public ProjectionOptions Projections { get; set; }

		public object Execute(IObjectRepository repository)
		{
			var parameters = new object[] {Sort, Paging, Fields, Filters, Projections};

			return Handle?.Invoke(repository, parameters);
		}
	}
}