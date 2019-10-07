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
using HQ.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace HQ.Platform.Operations.Models
{
	public class ConfigurationService
	{
		private readonly IConfigurationRoot _root;

		public ConfigurationService(IConfigurationRoot root)
		{
			_root = root;
		}

		public object Get(Type type, string section)
		{
			var config = _root.GetSection(section?.Replace("/", ":"));
			if (config == null)
				return null;

			var template = Activator.CreateInstance(type);
			config.FastBind(template);

			return template;
		}
	}
}