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
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace HQ.Common.AspNetCore.Mvc
{
	public class DynamicComponentConvention : IApplicationModelConvention
	{
		private readonly IEnumerable<IDynamicComponent> _components;

		public DynamicComponentConvention(IEnumerable<IDynamicComponent> components) => _components = components;

		public void Apply(ApplicationModel application)
		{
			foreach (var component in _components)
			{
				var typeNames = component.ControllerTypes.Select(x =>
					x.Name.Contains('`') ? x.Name : x.Name.Replace(nameof(Controller), string.Empty)).ToList();

				foreach (var controller in application.Controllers)
				{
					if (!typeNames.Contains(controller.ControllerName)) continue;

					var template = component.Namespace();
					var prefix = new AttributeRouteModel(new RouteAttribute(template));

					foreach (var selector in controller.Selectors)
						selector.AttributeRouteModel = selector.AttributeRouteModel != null
							? AttributeRouteModel.CombineAttributeRouteModel(prefix, selector.AttributeRouteModel)
							: prefix;
				}
			}
		}
	}
}