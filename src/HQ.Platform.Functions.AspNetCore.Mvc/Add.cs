using System.Collections.Generic;
using System.Reflection;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Extensions.Scheduling;
using HQ.Platform.Api.Conventions;
using HQ.Platform.Functions.AspNetCore.Mvc.Controllers;
using HQ.Platform.Functions.AspNetCore.Mvc.Models;
using HQ.Platform.Operations;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore.Extensions;
using HQ.Platform.Security.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Functions.AspNetCore.Mvc
{
    public static class Add
    {
        public static IMvcBuilder AddBackgroundTasksApi(this IMvcBuilder mvcBuilder, IConfiguration securityConfig, string rootPath = null)
        {
            var services = mvcBuilder.Services;

            services.AddBackgroundTasks();
            
            var securityOptions = new SecurityOptions();
            securityConfig.Bind(securityOptions);

            services.AddAuthorization(x =>
            {
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
