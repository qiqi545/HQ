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

using Microsoft.Net.Http.Headers;

namespace HQ.Data.Contracts.Configuration
{
	public class QueryOptions
	{
		public int PerPageDefault { get; set; } = 10;
		public int PerPageMax { get; set; } = 100;
		public int ProjectionMaxFields { get; set; } = 6;

		public string SortOperator { get; set; } = "sort";
		public string PageOperator { get; set; } = "page";
		public string PerPageOperator { get; set; } = "perPage";
		public string FieldsOperator { get; set; } = "fields";
		public string FilterOperator { get; set; } = "filter";
		public string ProjectionOperator { get; set; } = "project";
		public string AfterOperator { get; set; } = "after";
		public string BeforeOperator { get; set; } = "before";

		public string TotalCountHeader { get; set; } = HeaderNames.TotalCount;
		public string TotalPagesHeader { get; set; } = HeaderNames.TotalPages;
	}
}