using System;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.Mvc.Security;
using HQ.Extensions.Logging;
using HQ.Extensions.Scheduling;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Platform.Api.Functions.AspNetCore.Mvc.Controllers;
using HQ.Platform.Api.Functions.AspNetCore.Mvc.Models;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore.Extensions;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Api.Functions.AspNetCore.Mvc
{
	public static class Add
    {
	    public static IServiceCollection AddBackgroundTasksApi(this IServiceCollection services, IConfiguration config)
	    {
		    return AddBackgroundTasksApi(services, config.Bind);
	    }

	    public static IServiceCollection AddBackgroundTasksApi(this IServiceCollection services, Action<BackgroundTaskOptions> configureTasks = null)
	    {
		    services.AddMvc()
			    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
			    .AddBackgroundTasksApi(configureTasks);

		    return services;
	    }
		
	    private static void AddBackgroundTasksApi(this IMvcBuilder mvcBuilder, Action<BackgroundTaskOptions> configureTasks = null)
	    {
		    mvcBuilder.Services.AddLocalTimestamps();
            mvcBuilder.Services.AddSafeLogging();
			
			mvcBuilder.Services.AddBackgroundTasks(configureTasks);
            mvcBuilder.AddControllerFeature<BackgroundTaskController>();
            mvcBuilder.AddComponentFeature<BackgroundTasksComponent, BackgroundTaskOptions>();

            mvcBuilder.Services.AddDynamicAuthorization();
			mvcBuilder.Services.AddAuthorization(x =>
            {
	            var serviceProvider = mvcBuilder.Services.BuildServiceProvider();
	            var options = serviceProvider.GetRequiredService<IOptions<SecurityOptions>>();

				x.AddPolicy(Constants.Security.Policies.ManageBackgroundTasks, b =>
                {
                    b.RequireAuthenticatedUserExtended(mvcBuilder.Services);
                    b.RequireClaimExtended(mvcBuilder.Services, options.Value.Claims.PermissionClaim, ClaimValues.ManageBackgroundTasks);
                });
            });
        }
    }
}
