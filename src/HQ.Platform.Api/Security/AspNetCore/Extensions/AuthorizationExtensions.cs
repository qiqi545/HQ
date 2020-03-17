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

using ActiveOptions;
using HQ.Platform.Api.Security.AspNetCore.Requirements;
using HQ.Platform.Api.Security.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Api.Security.AspNetCore.Extensions
{
	public static class AuthorizationExtensions
	{
		public static AuthorizationPolicyBuilder RequireAuthenticatedUserExtended(
			this AuthorizationPolicyBuilder builder, IServiceCollection services)
		{
			var serviceProvider = services.BuildServiceProvider();
			var security = serviceProvider.GetRequiredService<IValidOptionsMonitor<SecurityOptions>>();
			var superUser = serviceProvider.GetRequiredService<IValidOptionsMonitor<SuperUserOptions>>();
			var requireAuthenticatedUser = new DenyAnonymousAuthorizationRequirementExtended(security, superUser);
			services.AddSingleton<IAuthorizationHandler>(requireAuthenticatedUser);
			builder.AddRequirements(requireAuthenticatedUser);
			return builder;
		}

		public static AuthorizationPolicyBuilder RequireClaimExtended(this AuthorizationPolicyBuilder builder,
			IServiceCollection services, string claimType, params string[] allowedValues)
		{
			var serviceProvider = services.BuildServiceProvider();
			var security = serviceProvider.GetRequiredService<IValidOptionsMonitor<SecurityOptions>>();
			var superUser = serviceProvider.GetRequiredService<IValidOptionsMonitor<SuperUserOptions>>();
			var requireClaim =
				new ClaimsAuthorizationRequirementExtended(security, superUser, claimType, allowedValues);
			services.AddSingleton<IAuthorizationHandler>(requireClaim);
			builder.AddRequirements(requireClaim);
			return builder;
		}

		public static AuthorizationPolicyBuilder RequireRoleExtended(this AuthorizationPolicyBuilder builder,
			IServiceCollection services, params string[] allowedRoles)
		{
			var serviceProvider = services.BuildServiceProvider();
			var monitor = serviceProvider.GetRequiredService<IValidOptionsMonitor<SecurityOptions>>();
			var superUser = serviceProvider.GetRequiredService<IValidOptionsMonitor<SuperUserOptions>>();
			var requireRole = new RolesAuthorizationRequirementExtended(monitor, superUser, allowedRoles);
			services.AddSingleton<IAuthorizationHandler>(requireRole);
			builder.AddRequirements(requireRole);
			return builder;
		}

		public static IServiceCollection AddDefaultAuthorization(this IServiceCollection services, string policyName,
			string permission)
		{
			services.AddAuthorization(x =>
			{
				if (x.GetPolicy(policyName) == null)
				{
					x.AddPolicy(policyName, b =>
					{
						var serviceProvider = services.BuildServiceProvider();
						var options = serviceProvider.GetRequiredService<IOptions<SecurityOptions>>();

						b.RequireAuthenticatedUserExtended(services);
						b.RequireClaimExtended(services, options.Value.Claims.PermissionClaim, permission);
					});
				}
			});
			return services;
		}
	}
}