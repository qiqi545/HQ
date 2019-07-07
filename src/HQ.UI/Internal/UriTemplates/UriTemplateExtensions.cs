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

using System.Collections.Generic;
using System.Reflection;

namespace HQ.UI.Internal.UriTemplates
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