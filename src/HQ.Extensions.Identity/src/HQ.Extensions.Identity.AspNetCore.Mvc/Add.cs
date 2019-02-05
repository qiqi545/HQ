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
using System.Collections.Generic;
using System.Reflection;
using HQ.Extensions.Identity.AspNetCore.Mvc.Configuration;
using HQ.Extensions.Identity.AspNetCore.Mvc.Controllers;
using HQ.Extensions.Identity.Models;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Platform.Api.Conventions;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore.Extensions;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Identity.AspNetCore.Mvc
{
    public static class Add
    {
        public static IMvcBuilder AddDynamicMvc(this IServiceCollection services, Action<MvcOptions> setupAction = null)
        {
            // See: https://github.com/aspnet/Mvc/issues/5992
            var controllerAssembly = Assembly.GetCallingAssembly();

            return services.AddMvc(o => { setupAction?.Invoke(o); })
                .AddApplicationPart(controllerAssembly)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public static IServiceCollection AddIdentityApi<TUser, TRole>(this IMvcBuilder mvc,
            IConfiguration identityConfig, IConfiguration securityConfig, Action<MvcOptions> setupAction = null)
            where TUser : IdentityUserExtended
            where TRole : IdentityRoleExtended
        {
            var services = mvc.Services;

            var options = new SecurityOptions();
            securityConfig.Bind(options);

            services.AddAuthorization(x =>
            {
                x.AddPolicy(Constants.Security.Policies.ManageUsers,
                    b =>
                    {
                        b.RequireClaimExtended(services, options, options.Claims.PermissionClaim,
                            ClaimValues.ManageUsers);
                    });
                x.AddPolicy(Constants.Security.Policies.ManageRoles,
                    b =>
                    {
                        b.RequireClaimExtended(services, options, options.Claims.PermissionClaim,
                            ClaimValues.ManageRoles);
                    });
            });

            services.Configure<IdentityApiOptions>(identityConfig);
            services.Configure<RazorViewEngineOptions>(x => { x.ViewLocationExpanders.Add(new DynamicViewLocationExpander<TUser>()); });

            mvc.AddControllers<TUser, TRole>();
            services.AddSingleton<IDynamicComponent>(r =>
            {
                var o = r.GetRequiredService<IOptions<IdentityApiOptions>>();
                return new IdentityApiComponent
                {
                    Namespace = () => o.Value.RootPath ?? string.Empty
                };
            });
            services.AddSingleton<IDynamicComponent>(r =>
            {
                var o = r.GetRequiredService<IOptions<SecurityOptions>>();
                return new TokensComponent
                {
                    Namespace = () => o.Value.Tokens?.Path ?? string.Empty
                };
            });

            return services;
        }

        private static IMvcBuilder AddControllers<TUser, TRole>(this IMvcBuilder mvc)
            where TUser : IdentityUserExtended
            where TRole : IdentityRoleExtended
        {
            var typeInfo = new List<TypeInfo>
            {
                typeof(TokenController<TUser>).GetTypeInfo(),
                typeof(UserController<TUser>).GetTypeInfo(),
                typeof(RoleController<TRole>).GetTypeInfo()
            };
            return mvc.ConfigureApplicationPartManager(x =>
            {
                x.ApplicationParts.Add(new DynamicControllerApplicationPart(typeInfo));
            });
        }
    }
}
