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
using ActiveRoutes;
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
using Constants = HQ.Common.Constants;

namespace HQ.Platform.Identity.Mvc
{
	public static class Add
	{
		public static IServiceCollection AddIdentityApi(this IServiceCollection services, IConfiguration apiConfig)
		{
			return AddIdentityApi<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication,
				string>(services, apiConfig.FastBind);
		}

		public static IServiceCollection AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(
			this IServiceCollection services, IConfiguration apiConfig)
			where TUser : IdentityUserExtended<TKey>
			where TRole : IdentityRoleExtended<TKey>
			where TTenant : IdentityTenant<TKey>
			where TApplication : IdentityApplication<TKey>
			where TKey : IEquatable<TKey>
		{
			return AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(services, apiConfig.FastBind);
		}

		public static IServiceCollection AddIdentityApi(this IServiceCollection services,
			Action<IdentityApiOptions> configureApi = null)
		{
			return AddIdentityApi<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication,
				string>(services, configureApi);
		}

		public static IServiceCollection AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(
			this IServiceCollection services, Action<IdentityApiOptions> configureApi = null)
			where TUser : IdentityUserExtended<TKey>
			where TRole : IdentityRoleExtended<TKey>
			where TTenant : IdentityTenant<TKey>
			where TApplication : IdentityApplication<TKey>
			where TKey : IEquatable<TKey>
		{
			services.AddMvcCore()
				.AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(configureApi);

			return services;
		}

		private static void AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(this IMvcCoreBuilder mvcBuilder,
			Action<IdentityApiOptions> configureApi = null)
			where TUser : IdentityUserExtended<TKey>
			where TRole : IdentityRoleExtended<TKey>
			where TTenant : IdentityTenant<TKey>
			where TApplication : IdentityApplication<TKey>
			where TKey : IEquatable<TKey>
		{
			if (configureApi != null)
				mvcBuilder.Services.Configure(configureApi);

			mvcBuilder.AddActiveRoute<TenantController<TTenant, TKey>, IdentityApiComponent, IdentityApiOptions>();
			mvcBuilder.AddActiveRoute<ApplicationController<TApplication, TKey>, IdentityApiComponent, IdentityApiOptions>();
			mvcBuilder.AddActiveRoute<UserController<TUser, TTenant, TKey>, IdentityApiComponent, IdentityApiOptions>();
			mvcBuilder.AddActiveRoute<RoleController<TRole, TKey>, IdentityApiComponent, IdentityApiOptions>();

			mvcBuilder.AddDefaultAuthorization(Constants.Security.Policies.ManageUsers, ClaimValues.ManageUsers);
			mvcBuilder.AddDefaultAuthorization(Constants.Security.Policies.ManageRoles, ClaimValues.ManageRoles);
			mvcBuilder.AddDefaultAuthorization(Constants.Security.Policies.ManageTenants, ClaimValues.ManageTenants);
			mvcBuilder.AddDefaultAuthorization(Constants.Security.Policies.ManageApplications, ClaimValues.ManageApplications);

			if (TokensEnabled(mvcBuilder))
				mvcBuilder.AddActiveRoute<TokenController<TUser, TTenant, TApplication, TKey>, IdentityApiComponent, IdentityApiOptions>();
		}

		private static bool TokensEnabled(this IMvcCoreBuilder mvcBuilder)
		{
			var serviceProvider = mvcBuilder.Services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IOptions<SecurityOptions>>().Value.Tokens.Enabled;
		}
	}
}