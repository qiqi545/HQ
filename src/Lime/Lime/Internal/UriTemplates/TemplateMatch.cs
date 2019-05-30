// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Lime.Internal.UriTemplates
{
	public class TemplateMatch
	{
		public string Key { get; set; }
		public UriTemplate Template { get; set; }
		public IDictionary<string, object> Parameters { get; set; }
	}
}