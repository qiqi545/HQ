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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HQ.Common.AspNetCore.Models
{
	[DebuggerDisplay("Url: {raw}")]
	public class MetaUrl
	{
		public string raw { get; set; }
		public string protocol { get; set; }
		public string[] host { get; set; }
		public string port { get; set; }
		public string[] path { get; set; }
		public List<MetaParameter> query { get; set; } = null;

		public static MetaUrl FromRaw(string url)
		{
			var result = new MetaUrl();

			// FIXME: add query parameters from parsed URL 

			if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
			{
				return new MetaUrl
				{
					raw = Uri.UnescapeDataString(uri.AbsoluteUri),
					protocol = uri.Scheme.ToLowerInvariant(),
					host = new[] { uri.Host },
					port = uri.Port.ToString(),
					path = uri.Segments.Where(x => !string.IsNullOrWhiteSpace(x))
						.Select(x => Uri.UnescapeDataString(x.Replace("/", "")))
						.ToArray()
				};
			}

			result.raw = Uri.UnescapeDataString(url);
			return result;
		}
	}
}