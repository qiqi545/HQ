// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

namespace HQ.Rosetta
{
	public class QueryOptions
	{
		public long PerPageDefault { get; set; } = 10;
		public long PerPageMax { get; set; } = 100;

		public string SortOperator { get; set; } = "sort";
		public string PageOperator { get; set; } = "page";
		public string PerPageOperator { get; set; } = "perPage";
		public string FieldsOperator { get; set; } = "fields";
		public string FilterOperator { get; set; } = "filter";
		public string ProjectionOperator { get; set; } = "project";
	}
}