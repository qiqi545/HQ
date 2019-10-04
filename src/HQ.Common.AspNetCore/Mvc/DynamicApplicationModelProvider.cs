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
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace HQ.Common.AspNetCore.Mvc
{
	public sealed class DynamicApplicationModelProvider : IApplicationModelProvider
	{
		private readonly IServiceProvider _serviceProvider;

		public DynamicApplicationModelProvider(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

		public void OnProvidersExecuting(ApplicationModelProviderContext context)
		{
			SetServiceProviders(context);
		}

		public void OnProvidersExecuted(ApplicationModelProviderContext context) { }

		public int Order { get; set; }

		private void SetServiceProviders(ApplicationModelProviderContext context)
		{
			foreach (var controllerModel in context.Result.Controllers)
			{
				foreach (var o in controllerModel.Attributes)
				{
					if (o is IDynamicAttribute attribute)
					{
						attribute.ServiceProvider = _serviceProvider;
					}
				}

				foreach (var a in controllerModel.Actions)
				{
					foreach (var o in a.Attributes)
					{
						if (o is IDynamicAttribute attribute)
						{
							attribute.ServiceProvider = _serviceProvider;
						}
					}
				}
			}
		}
	}
}