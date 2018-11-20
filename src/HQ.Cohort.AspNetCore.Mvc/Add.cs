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
using HQ.Cohort.AspNetCore.Mvc.Configuration;
using HQ.Cohort.AspNetCore.Mvc.Controllers;
using HQ.Cohort.Configuration;
using HQ.Cohort.Models;
using HQ.Cohort.Services;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Common.Models;
using HQ.Tokens;
using HQ.Tokens.Configuration;
using HQ.Tokens.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Cohort.AspNetCore.Mvc
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
            where TUser : IdentityUser
            where TRole : IdentityRole
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

            services.AddScoped<IUserService<TUser>, UserService<TUser>>();
            services.AddSingleton<IServerTimestampService, LocalServerTimestampService>();

            return services;
        }

        public static IdentityBuilder AddIdentity<TUser>(this IServiceCollection services, IConfiguration configuration)
            where TUser : class
        {
            AddIdentityPreamble<TUser>(services);

            return services.AddIdentityCoreExtended<TUser>(configuration);
        }

        public static IdentityBuilder AddIdentity<TUser>(this IServiceCollection services,
            Action<IdentityOptionsExtended> setupAction)
            where TUser : class
        {
            AddIdentityPreamble<TUser>(services);

            return services.AddIdentityCoreExtended<TUser>(o => { setupAction?.Invoke(o); });
        }

        private static void AddIdentityPreamble<TUser>(IServiceCollection services) where TUser : class
        {
            var authBuilder = AuthenticationServiceCollectionExtensions.AddAuthentication(services, o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            });

            var cookiesBuilder = authBuilder.AddIdentityCookies(o => { });
        }

        private static IMvcBuilder AddControllers<TUser, TRole>(this IMvcBuilder mvc)
            where TUser : IdentityUser
            where TRole : IdentityRole
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
