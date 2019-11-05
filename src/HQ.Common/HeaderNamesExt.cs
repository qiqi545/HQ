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

// ReSharper disable once CheckNamespace
namespace Microsoft.Net.Http.Headers
{
	public static class HeaderNamesExt
	{
		/// <summary>
		///     See: https://www.w3.org/TR/server-timing/
		/// </summary>
		public const string ServerTiming = "Server-Timing";

		/// <summary>
		///     See: https://www.w3.org/TR/server-timing/
		/// </summary>
		public const string TimingAllowOrigin = "Timing-Allow-Origin";

		/// <summary>
		///     See: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#75-standard-request-headers
		/// </summary>
		public const string Prefer = "Prefer";

		/// <summary>
		///     See: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#75-standard-request-headers
		/// </summary>
		public const string PreferenceApplied = "Preference-Applied";

		public const string MethodOverride = "X-HTTP-Method-Override";
		public const string Action = "X-Action";
		public const string TotalCount = "X-Total-Count";
		public const string TotalPages = "X-Total-Pages";

		public const string Link = "Link";
	}
}