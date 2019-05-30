// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace Lime.Internal.UriTemplates
{
	public static class UriTemplateExtensions
	{
		public static UriTemplate AddParameter(this UriTemplate template, string name, object value)
		{
			template.SetParameter(name, value);

			return template;
		}

		public static UriTemplate AddParameters(this UriTemplate template, object parametersObject)
		{
			if (parametersObject != null)
			{
				IEnumerable<PropertyInfo> properties;
#if NETSTANDARD1_0
                var type = parametersObject.GetType().GetTypeInfo();
                properties = type.DeclaredProperties.Where(p=> p.CanRead);
#else
				properties = parametersObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
#endif

				foreach (var propinfo in properties)
					template.SetParameter(propinfo.Name, propinfo.GetValue(parametersObject, null));
			}

			return template;
		}

		public static UriTemplate AddParameters(this UriTemplate uriTemplate,
			IDictionary<string, object> linkParameters)
		{
			if (linkParameters != null)
				foreach (var parameter in linkParameters)
					uriTemplate.SetParameter(parameter.Key, parameter.Value);
			return uriTemplate;
		}
	}
}