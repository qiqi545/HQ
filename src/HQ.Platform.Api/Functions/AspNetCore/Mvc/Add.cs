using System;
using System.Collections.Generic;
using System.Reflection;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Extensions.Logging;
using HQ.Extensions.Scheduling;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Platform.Functions.AspNetCore.Mvc.Controllers;
using HQ.Platform.Functions.AspNetCore.Mvc.Models;
using HQ.Platform.Runtime.Rest;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore;
using HQ.Platform.Security.AspNetCore.Extensions;
using HQ.Platform.Security.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Functions.AspNetCore.Mvc
{
    public static class Add
    {
        public static IMvcBuilder AddBackgroundTasksApi(this IMvcBuilder mvcBuilder, IConfiguration securityConfig, IConfiguration backgroundTasksConfig, string rootPath = "/ops")
        {
            return AddBackgroundTasksApi(mvcBuilder, securityConfig.Bind, backgroundTasksConfig.Bind, rootPath);
        }

        public static IMvcBuilder AddBackgroundTasksApi(this IMvcBuilder mvcBuilder, Action<SecurityOptions> configureSecurity = null, Action<BackgroundTaskOptions> configureTasks = null, string rootPath = "/ops")
        {
            var services = mvcBuilder.Services;

            if (configureSecurity != null)
                services.Configure(configureSecurity);

			services.TryAddSingleton<IServerTimestampService, LocalServerTimestampService>();
			services.AddSafeLogging();

			services.AddSecurityPolicies(configureSecurity);
			services.AddBackgroundTasks(configureTasks);
            services.AddRestRuntime();

            mvcBuilder.AddFeature<BackgroundTaskController>();

			services.AddAuthorization(x =>{
                var securityOptions = new SecurityOptions();
                configureSecurity?.Invoke(securityOptions);

                x.AddPolicy(Constants.Security.Policies.ManageBackgroundTasks, b =>
                {
                    b.RequireAuthenticatedUserExtended(services, securityOptions);
                    b.RequireClaimExtended(services, securityOptions, securityOptions.Claims.PermissionClaim, ClaimValues.ManageBackgroundTasks);
                });
            });

            mvcBuilder.ConfigureApplicationPartManager(x =>
            {
                var typeInfo = new List<TypeInfo> { typeof(BackgroundTaskController).GetTypeInfo() };

                x.ApplicationParts.Add(new DynamicControllerApplicationPart(typeInfo));
            });

            services.AddSingleton<IDynamicComponent>(r =>
            {
                return new BackgroundTasksComponent { Namespace = () =>
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
