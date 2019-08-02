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
using System.Linq;
using System.Text.RegularExpressions;

namespace HQ.UI.Internal.UriTemplates
{
	public static class UriExtensions
	{
		public static UriTemplate MakeTemplate(this Uri uri)
		{
			var parameters = uri.GetQueryStringParameters();
			return MakeTemplate(uri, parameters);
		}

		public static UriTemplate MakeTemplate(this Uri uri, IDictionary<string, object> parameters)
		{
			var target = uri.GetComponents(UriComponents.AbsoluteUri
			                               & ~UriComponents.Query
			                               & ~UriComponents.Fragment, UriFormat.Unescaped);
			var template = new UriTemplate($"{target}{{?{string.Join(",", parameters.Keys.ToArray())}}}");
			template.AddParameters(parameters);

			return template;
		}

		public static Dictionary<string, object> GetQueryStringParameters(this Uri target)
		{
			var uri = target;
			var parameters = new Dictionary<string, object>();

			var reg = new Regex(
				@"([-A-Za-z0-9._~]*)=([^&]*)&?"); // Unreserved characters: http://tools.ietf.org/html/rfc3986#section-2.3
			foreach (Match m in reg.Matches(uri.Query))
			{
				var key = m.Groups[1].Value.ToLowerInvariant();
				var value = m.Groups[2].Value;
				parameters.Add(key, value);
			}

			return parameters;
		}
	}
}