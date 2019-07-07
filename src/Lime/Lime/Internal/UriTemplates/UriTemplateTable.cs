// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Lime.Internal.UriTemplates
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