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
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.AspNetCore.Mvc.Security;
using HQ.Extensions.Options;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Models;
using HQ.Platform.Identity.Mvc.Controllers;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore.Extensions;
using HQ.Platform.Security.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Identity.Mvc
{
    public static class Add
    {
	    public static IServiceCollection AddIdentityApi(this IServiceCollection services, IConfiguration apiConfig)
	    {
		    return AddIdentityApi<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication, string>(services, apiConfig.FastBind);
	    }

		public static IServiceCollection AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(this IServiceCollection services, IConfiguration apiConfig)
		    where TUser : IdentityUserExtended<TKey>
		    where TRole : IdentityRoleExtended<TKey>
		    where TTenant : IdentityTenant<TKey>
		    where TApplication : IdentityApplication<TKey>
		    where TKey : IEquatable<TKey>
		{
		    return AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(services, apiConfig.FastBind);
	    }

		public static IServiceCollection AddIdentityApi(this IServiceCollection services, Action<IdentityApiOptions> configureApi = null)
		{
			return AddIdentityApi<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication, string>(services, configureApi);
		}

		public static IServiceCollection AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(this IServiceCollection services, Action<IdentityApiOptions> configureApi = null)
		    where TUser : IdentityUserExtended<TKey>
		    where TRole : IdentityRoleExtended<TKey>
		    where TTenant : IdentityTenant<TKey>
		    where TApplication : IdentityApplication<TKey>
		    where TKey : IEquatable<TKey>
	    {
		    services.AddMvcCommon()
			    .AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(configureApi);

		    return services;
	    }

		private static void AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(this IMvcBuilder mvcBuilder,
			Action<IdentityApiOptions> configureApi = null)
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
            where TApplication : IdentityApplication<TKey>
            where TKey : IEquatable<TKey>
        {
			if(configureApi != null)
				mvcBuilder.Services.Configure(configureApi);

			mvcBuilder.Services.AddDynamicAuthorization();

			mvcBuilder.AddControllerFeature<TenantController<TTenant, TKey>>();
			mvcBuilder.AddControllerFeature<ApplicationController<TApplication, TKey>>();
			mvcBuilder.AddControllerFeature<UserController<TUser, TTenant, TKey>>();
			mvcBuilder.AddControllerFeature<RoleController<TRole, TKey>>();

			mvcBuilder.AddDefaultAuthorization(Constants.Security.Policies.ManageUsers, ClaimValues.ManageUsers);
			mvcBuilder.AddDefaultAuthorization(Constants.Security.Policies.ManageRoles, ClaimValues.ManageRoles);
			mvcBuilder.AddDefaultAuthorization(Constants.Security.Policies.ManageTenants, ClaimValues.ManageTenants);
			mvcBuilder.AddDefaultAuthorization(Constants.Security.Policies.ManageApplications, ClaimValues.ManageApplications);

			mvcBuilder.AddComponentFeature<IdentityApiComponent, IdentityApiOptions>();

			if (TokensEnabled(mvcBuilder))
			{
				mvcBuilder.AddControllerFeature<TokenController<TUser, TTenant, TApplication, TKey>>();

				// FIXME:
				// mvcBuilder.AddComponentFeature<TokensComponent, TokenOptions>();
				mvcBuilder.Services.AddSingleton<IDynamicComponent>(r =>
				{
					return new TokensComponent
					{
						RouteTemplate = () =>
						{
							var o = r.GetRequiredService<IOptionsMonitor<SecurityOptions>>();
							return o.CurrentValue.Tokens?.RootPath ?? string.Empty;
						}
					};
				});
			}
        }

		private static bool TokensEnabled(this IMvcBuilder mvcBuilder)
		{
			return mvcBuilder.Services.BuildServiceProvider().GetRequiredService<IOptions<SecurityOptions>>().Value.Tokens.Enabled;
		}
    }
}
