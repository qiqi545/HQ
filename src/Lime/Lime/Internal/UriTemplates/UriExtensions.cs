// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lime.Internal.UriTemplates
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