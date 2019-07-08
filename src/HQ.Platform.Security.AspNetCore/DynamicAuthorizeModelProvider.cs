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
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace HQ.Platform.Security.AspNetCore
{
	internal sealed class DynamicAuthorizeModelProvider : IApplicationModelProvider
	{
		private readonly IServiceProvider _serviceProvider;

		public DynamicAuthorizeModelProvider(IServiceProvider urlHelperFactory)
		{
			_serviceProvider = urlHelperFactory;
		}

		public void OnProvidersExecuting(ApplicationModelProviderContext context) { }

		public void OnProvidersExecuted(ApplicationModelProviderContext context)
		{
			foreach (var controllerModel in context.Result.Controllers)
			{
				controllerModel.Attributes
					.OfType<DynamicAuthorizeAttribute>().ToList()
					.ForEach(a => a.ServiceProvider = _serviceProvider);

				controllerModel.Actions.SelectMany(a => a.Attributes)
					.OfType<DynamicAuthorizeAttribute>().ToList()
					.ForEach(a => a.ServiceProvider = _serviceProvider);
			}
		}

		public int Order { get; set; }
	}
}