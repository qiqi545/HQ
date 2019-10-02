using System;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.AspNetCore.Mvc.Security;
using HQ.Data.Contracts.Mvc.Security;
using HQ.Extensions.Logging;
using HQ.Extensions.Options;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Platform.Api.Functions.AspNetCore.Mvc.Controllers;
using HQ.Platform.Api.Functions.AspNetCore.Mvc.Models;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Api.Functions.AspNetCore.Mvc
{
	public static class Add
    {
	    public static IServiceCollection AddBackgroundTasksApi(this IServiceCollection services, IConfiguration config)
	    {
		    return AddBackgroundTasksApi(services, config.FastBind);
	    }

	    public static IServiceCollection AddBackgroundTasksApi(this IServiceCollection services, Action<BackgroundTaskOptions> configureTasks = null)
	    {
		    services.AddMvcCommon()
			    .AddBackgroundTasksApi(configureTasks);
		    return services;
	    }
		
	    private static void AddBackgroundTasksApi(this IMvcBuilder mvcBuilder, Action<BackgroundTaskOptions> configureTasks = null)
	    {
		    mvcBuilder.Services.Configure(configureTasks);

			mvcBuilder.Services.AddLocalTimestamps();
            mvcBuilder.Services.AddSafeLogging();
			
            mvcBuilder.AddControllerFeature<BackgroundTaskController>();
            mvcBuilder.AddComponentFeature<BackgroundTasksComponent, BackgroundTaskOptions>();

            mvcBuilder.Services.AddDynamicAuthorization();
			mvcBuilder.AddDefaultAuthorization(Constants.Security.Policies.ManageBackgroundTasks, ClaimValues.ManageBackgroundTasks);
        }
    }
}
