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
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using TypeKitchen;

namespace HQ.Platform.Security.AspNetCore
{
	public class DynamicFilterProvider : DefaultFilterProvider
	{
		private readonly IEnumerable<IDynamicComponent> _components;

		public DynamicFilterProvider(IEnumerable<IDynamicComponent> components)
		{
			_components = components;
		}

		public override void ProvideFilter(FilterProviderContext context, FilterItem filterItem)
		{
			if (!(context.ActionContext is ControllerContext controllerContext))
			{
				base.ProvideFilter(context, filterItem);
				return;
			}

			var controllerType = ResolveComponentControllerType(controllerContext);
			if (controllerType == null)
			{
				base.ProvideFilter(context, filterItem);
				return;
			}

			var attributes = controllerType.GetAttributes<DynamicAuthorizeAttribute>().ToList();
			if (attributes.Count == 0)
			{
				base.ProvideFilter(context, filterItem);
				return;
			}

			foreach (var attribute in attributes)
				attribute.Resolve(context.ActionContext.HttpContext.RequestServices);

			var authorizeFilter = new AuthorizeFilter(attributes);
			foreach (var result in context.Results)
			{
				if (!(result.Descriptor.Filter is AuthorizeFilter existing))
					continue;
				result.Filter = existing;
				base.ProvideFilter(context, filterItem);
				return;
			}

			context.Results.Add(new FilterItem(new FilterDescriptor(authorizeFilter, FilterScope.Controller)));
			base.ProvideFilter(context, filterItem);
		}

		private Type ResolveComponentControllerType(ControllerContext context)
		{
			foreach (var entry in _components)
			{
				var type = context.ActionDescriptor.ControllerTypeInfo.AsType();
				if (type.IsGenericType)
					type = type.GetGenericTypeDefinition();

				if (entry.ControllerTypes.Contains(type))
				{
					return type;
				}
			}
			return default;
		}
	}
}