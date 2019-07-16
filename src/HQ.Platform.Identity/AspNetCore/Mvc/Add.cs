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
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Platform.Identity.AspNetCore.Mvc.Controllers;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Models;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore;
using HQ.Platform.Security.AspNetCore.Extensions;
using HQ.Platform.Security.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Identity.AspNetCore.Mvc
{
    public static class Add
    {
	    public static IMvcBuilder AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(this IMvcBuilder mvcBuilder,
		    IConfiguration apiConfig, IConfiguration securityConfig)
		    where TUser : IdentityUserExtended<TKey>
		    where TRole : IdentityRoleExtended<TKey>
		    where TTenant : IdentityTenant<TKey>
		    where TApplication : IdentityApplication<TKey>
		    where TKey : IEquatable<TKey>
	    {
		    return mvcBuilder.AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(apiConfig.Bind,
			    securityConfig.Bind);
	    }

	    public static IMvcBuilder AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(this IMvcBuilder mvcBuilder, Action<IdentityApiOptions> configureApi = null, Action<SecurityOptions> configureSecurity = null)
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
            where TApplication : IdentityApplication<TKey>
            where TKey : IEquatable<TKey>
        {
            var services = mvcBuilder.Services;

            var options = new SecurityOptions();
            configureSecurity?.Invoke(options);
			
            services.Configure(configureApi);

			services.AddSecurityPolicies(configureSecurity);
			mvcBuilder.AddControllerFeature<IdentityApplication>();

			services.AddAuthorization(x =>
            {
                x.AddPolicy(Constants.Security.Policies.ManageUsers,
                    b =>
                    {
                        b.RequireAuthenticatedUserExtended(services, options);
                        b.RequireClaimExtended(services, options, options.Claims.PermissionClaim,
                            ClaimValues.ManageUsers);
                    });
                x.AddPolicy(Constants.Security.Policies.ManageRoles,
                    b =>
                    {
                        b.RequireAuthenticatedUserExtended(services, options);
                        b.RequireClaimExtended(services, options, options.Claims.PermissionClaim,
                            ClaimValues.ManageRoles);
                    });
                x.AddPolicy(Constants.Security.Policies.ManageTenants,
                    b =>
                    {
                        b.RequireAuthenticatedUserExtended(services, options);
                        b.RequireClaimExtended(services, options, options.Claims.PermissionClaim,
                            ClaimValues.ManageTenants);
                    });
                x.AddPolicy(Constants.Security.Policies.ManageApplications,
                    b =>
                    {
                        b.RequireAuthenticatedUserExtended(services, options);
                        b.RequireClaimExtended(services, options, options.Claims.PermissionClaim,
                            ClaimValues.ManageApplications);
                    });
            });
			
            mvcBuilder.AddControllers<TUser, TRole, TTenant, TApplication, TKey>();

            services.AddSingleton<IDynamicComponent>(r =>
            {
                var o = r.GetRequiredService<IOptions<IdentityApiOptions>>();
                return new IdentityApiComponent {RouteTemplate = () => o.Value.RootPath ?? string.Empty};
            });

            services.AddSingleton<IDynamicComponent>(r =>
            {
                var o = r.GetRequiredService<IOptions<SecurityOptions>>();
                return new TokensComponent {RouteTemplate = () => o.Value.Tokens?.Path ?? string.Empty};
            });

            return mvcBuilder;
        }

        private static IMvcBuilder AddControllers<TUser, TRole, TTenant, TApplication, TKey>(this IMvcBuilder mvc)
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
            where TApplication : IdentityApplication<TKey>
            where TKey : IEquatable<TKey>
        {
            var typeInfo = new List<TypeInfo>
            {
                typeof(TokenController<TUser, TTenant, TApplication, TKey>).GetTypeInfo(),
                typeof(TenantController<TTenant, TKey>).GetTypeInfo(),
                typeof(ApplicationController<TApplication, TKey>).GetTypeInfo(),
                typeof(UserController<TUser, TTenant, TKey>).GetTypeInfo(),
                typeof(RoleController<TRole, TKey>).GetTypeInfo()
            };
            return mvc.ConfigureApplicationPartManager(x =>
            {
                x.ApplicationParts.Add(new DynamicControllerApplicationPart(typeInfo));
            });
        }
    }
}
