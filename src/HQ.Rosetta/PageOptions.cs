// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;

namespace HQ.Rosetta
{
	public class PageOptions : IQueryValidator
	{
		public long Page { get; set; }
		public long PerPage { get; set; }

		public bool Validate(Type type, QueryOptions options, out IEnumerable<Error> errors)
		{
			var list = new List<Error>();

			if (Page < 1) list.Add(new Error(ErrorStrings.PageRangeInvalid));

			if (PerPage > options.PerPageMax) list.Add(new Error(ErrorStrings.PerPageTooHigh));

			errors = list;
			return list.Count == 0;
		}
	}
}