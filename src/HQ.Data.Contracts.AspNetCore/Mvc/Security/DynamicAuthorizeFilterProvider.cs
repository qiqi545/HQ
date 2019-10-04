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
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.AspNetCore.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Data.Contracts.AspNetCore.Mvc.Security
{
	public sealed class DynamicAuthorizeFilterProvider : DefaultFilterProvider
	{
		private readonly IEnumerable<IDynamicComponent> _components;

		public DynamicAuthorizeFilterProvider(IEnumerable<IDynamicComponent> components) => _components = components;

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

			RemoveUnrelatedFilters(context);

			var attributes = context.ActionContext.ActionDescriptor.EndpointMetadata
				.OfType<DynamicAuthorizeAttribute>();
			// ReSharper disable once PossibleMultipleEnumeration
			if (!attributes.Any())
			{
				base.ProvideFilter(context, filterItem);
				return;
			}

			var serviceProvider = context.ActionContext.HttpContext.RequestServices;

			// ReSharper disable once PossibleMultipleEnumeration
			foreach (var attribute in attributes)
			{
				attribute.Resolve(serviceProvider);
			}

			var index = -1;
			for (var i = 0; i < context.Results.Count; i++)
			{
				var result = context.Results[i];
				if (!(result.Descriptor.Filter is DynamicAuthorizeFilter existing))
					continue;
				result.Filter = existing;
				index = i;
				break;
			}

			if (index > -1)
			{
				base.ProvideFilter(context, filterItem);
				return;
			}

			var policyProvider = serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>();
			// ReSharper disable once PossibleMultipleEnumeration
			var authorize = new DynamicAuthorizeFilter(policyProvider, attributes);
			var descriptor = new FilterDescriptor(authorize, FilterScope.Controller);
			var filter = new FilterItem(descriptor) {Filter = authorize};
			context.Results.Add(filter);

			base.ProvideFilter(context, filterItem);
		}

		private static void RemoveUnrelatedFilters(FilterProviderContext context)
		{
			for (var i = context.Results.Count - 1; i >= 0; i--)
			{
				var result = context.Results[i];
				if (!(result.Descriptor.Filter is AuthorizeFilter) ||
				    result.Descriptor.Filter is DynamicAuthorizeFilter)
					continue;
				context.Results.Remove(result);
			}
		}

		private Type ResolveComponentControllerType(ControllerContext context)
		{
			var type = context.ActionDescriptor.ControllerTypeInfo.AsType();
			foreach (var entry in _components)
			{
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