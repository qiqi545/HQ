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
using ActiveLogging;
using ActiveRoutes;
using HQ.Common;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Platform.Api.Functions.AspNetCore.Mvc.Controllers;
using HQ.Platform.Api.Functions.AspNetCore.Mvc.Models;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Constants = HQ.Common.Constants;
using ActiveOptions;

namespace HQ.Platform.Api.Functions.AspNetCore.Mvc
{
	public static class Add
	{
		public static IServiceCollection AddBackgroundTasksApi(this IServiceCollection services, IConfiguration config)
		{
			return AddBackgroundTasksApi(services, config.FastBind);
		}

		public static IServiceCollection AddBackgroundTasksApi(this IServiceCollection services,
			Action<BackgroundTaskOptions> configureTasks = null)
		{
			services.AddMvcCore().AddBackgroundTasksApi(configureTasks);
			return services;
		}

		public static IMvcCoreBuilder AddBackgroundTasksApi(this IMvcCoreBuilder mvcBuilder, IConfiguration config)
		{
			return AddBackgroundTasksApi(mvcBuilder, config.FastBind);
		}

		public static IMvcCoreBuilder AddBackgroundTasksApi(this IMvcCoreBuilder mvcBuilder, Action<BackgroundTaskOptions> configureTasks = null)
		{
			mvcBuilder.Services.Configure(configureTasks);
			mvcBuilder.Services.AddLocalTimestamps();
			mvcBuilder.Services.AddSafeLogging();

			mvcBuilder.AddActiveRoute<BackgroundTaskController, BackgroundTasksComponent, BackgroundTaskOptions>();
			mvcBuilder.AddDefaultAuthorization(Constants.Security.Policies.ManageBackgroundTasks, ClaimValues.ManageBackgroundTasks);

			return mvcBuilder;
		}
	}
}