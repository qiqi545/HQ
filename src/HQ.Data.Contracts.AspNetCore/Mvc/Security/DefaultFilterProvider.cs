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
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Data.Contracts.AspNetCore.Mvc.Security
{
	public class DefaultFilterProvider : IFilterProvider
	{
		public int Order => -1000;
		
		public void OnProvidersExecuting(FilterProviderContext context)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));
			if (context.ActionContext.ActionDescriptor.FilterDescriptors == null)
				return;

			for (var i = 0; i < context.Results.Count; i++)
			{
				var result = context.Results[i];
				ProvideFilter(context, result);
			}
		}

		public virtual void OnProvidersExecuted(FilterProviderContext context) { }

		public virtual void ProvideFilter(FilterProviderContext context, FilterItem filterItem)
		{
			if (filterItem.Filter != null)
				return;
			var filter = filterItem.Descriptor.Filter;
			var filterFactory = filter as IFilterFactory;
			if (filterFactory == null)
			{
				filterItem.Filter = filter;
				filterItem.IsReusable = true;
			}
			else
			{
				IServiceProvider requestServices = context.ActionContext.HttpContext.RequestServices;
				filterItem.Filter = filterFactory.CreateInstance(requestServices);
				filterItem.IsReusable = filterFactory.IsReusable;
				if (filterItem.Filter == null)
					throw new InvalidOperationException("FormatTypeMethodMustReturnNotNullValue"); // FIXME need a real message here
				ApplyFilterToContainer(filterItem.Filter, filterFactory);
			}
		}

		private static void ApplyFilterToContainer(object actualFilter, IFilterMetadata filterMetadata)
		{
			var filterContainer = actualFilter as IFilterContainer;
			if (filterContainer == null)
				return;
			filterContainer.FilterDefinition = filterMetadata;
		}
	}
}