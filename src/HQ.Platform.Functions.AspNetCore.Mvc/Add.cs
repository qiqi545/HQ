using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Extensions.Logging;
using HQ.Extensions.Scheduling;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Platform.Api.Conventions;
using HQ.Platform.Functions.AspNetCore.Mvc.Controllers;
using HQ.Platform.Functions.AspNetCore.Mvc.Models;
using HQ.Platform.Operations;
using HQ.Platform.Runtime.Rest;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore.Extensions;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Mvc;
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
            services.AddBackgroundTasks(configureTasks);
            services.AddRestRuntime();

			// See: https://github.com/aspnet/Mvc/issues/5992
			mvcBuilder.AddApplicationPart(typeof(BackgroundTaskController).Assembly);
			services.AddOptions<MvcOptions>()
				.Configure<IEnumerable<IDynamicComponent>>((o, x) =>
				{
					if (o.Conventions.FirstOrDefault(c => c.GetType() == typeof(DynamicComponentConvention)) == null)
						o.Conventions.Add(new DynamicComponentConvention(x));
				});

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
                    var o = r.GetRequiredService<IOptions<OperationsApiOptions>>();
                    return o.Value.RootPath ?? string.Empty;
                }};
            });

            return mvcBuilder;
        }
    }
}
