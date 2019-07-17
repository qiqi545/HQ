using System;
using System.Collections.Generic;
using System.Reflection;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Extensions.Logging;
using HQ.Extensions.Scheduling;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Platform.Api.Functions.AspNetCore.Mvc.Controllers;
using HQ.Platform.Api.Functions.AspNetCore.Mvc.Models;
using HQ.Platform.Runtime.Rest;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore;
using HQ.Platform.Security.AspNetCore.Extensions;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Api.Functions.AspNetCore.Mvc
{
	public static class Add
    {
	    public static IServiceCollection AddBackgroundTasksApi(this IServiceCollection services, IConfiguration securityConfig, IConfiguration backgroundTasksConfig, string rootPath = "/ops")
	    {
		    return AddBackgroundTasksApi(services, securityConfig.Bind, backgroundTasksConfig.Bind, rootPath);
	    }

	    public static IServiceCollection AddBackgroundTasksApi(this IServiceCollection services,
		    Action<SecurityOptions> configureSecurity = null, Action<BackgroundTaskOptions> configureTasks = null,
		    string rootPath = "/ops")
	    {
		    services.AddMvc()
			    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
			    .AddBackgroundTasksApi(configureSecurity, configureTasks, rootPath);

		    return services;
	    }

	    private static IMvcBuilder AddBackgroundTasksApi(this IMvcBuilder mvcBuilder, IConfiguration securityConfig, IConfiguration backgroundTasksConfig, string rootPath = "/ops")
        {
            return AddBackgroundTasksApi(mvcBuilder, securityConfig.Bind, backgroundTasksConfig.Bind, rootPath);
        }

	    private static IMvcBuilder AddBackgroundTasksApi(this IMvcBuilder mvcBuilder, Action<SecurityOptions> configureSecurity = null, Action<BackgroundTaskOptions> configureTasks = null, string rootPath = "/ops")
        {
            if (configureSecurity != null)
	            mvcBuilder.Services.Configure(configureSecurity);

            mvcBuilder.Services.TryAddSingleton<IServerTimestampService, LocalServerTimestampService>();
            mvcBuilder.Services.AddSafeLogging();

			mvcBuilder.Services.AddDynamicAuthorization();
			mvcBuilder.Services.AddBackgroundTasks(configureTasks);
			mvcBuilder.Services.AddRestRuntime();

            mvcBuilder.AddControllerFeature<BackgroundTaskController>();

            mvcBuilder.Services.AddAuthorization(x =>{
                var securityOptions = new SecurityOptions();
                configureSecurity?.Invoke(securityOptions);

                x.AddPolicy(Constants.Security.Policies.ManageBackgroundTasks, b =>
                {
                    b.RequireAuthenticatedUserExtended(mvcBuilder.Services);
                    b.RequireClaimExtended(mvcBuilder.Services, securityOptions.Claims.PermissionClaim, ClaimValues.ManageBackgroundTasks);
                });
            });

            mvcBuilder.ConfigureApplicationPartManager(x =>
            {
                var typeInfo = new List<TypeInfo> { typeof(BackgroundTaskController).GetTypeInfo() };

                x.ApplicationParts.Add(new DynamicControllerApplicationPart(typeInfo));
            });

            mvcBuilder.Services.AddSingleton<IDynamicComponent>(r =>
            {
                return new BackgroundTasksComponent { RouteTemplate = () =>
                {
                    if (!string.IsNullOrWhiteSpace(rootPath))
                        return rootPath;
                    var o = r.GetRequiredService<IOptions<BackgroundTaskOptions>>();
                    return o.Value.RootPath ?? string.Empty;
                }};
            });

            return mvcBuilder;
        }
    }
}
