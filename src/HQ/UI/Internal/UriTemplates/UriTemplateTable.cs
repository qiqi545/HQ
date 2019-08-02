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

namespace HQ.UI.Internal.UriTemplates
{
	public class UriTemplateTable
	{
		private readonly Dictionary<string, UriTemplate> _Templates = new Dictionary<string, UriTemplate>();

		public UriTemplate this[string key] => _Templates.TryGetValue(key, out var value) ? value : null;

		public void Add(string key, UriTemplate template)
		{
			_Templates.Add(key, template);
		}

		public TemplateMatch Match(Uri url, QueryStringParameterOrder order = QueryStringParameterOrder.Strict)
		{
			foreach (var template in _Templates)
			{
				var parameters = template.Value.GetParameters(url, order);
				if (parameters != null)
					return new TemplateMatch {Key = template.Key, Parameters = parameters, Template = template.Value};
			}

			return null;
		}
	}
}