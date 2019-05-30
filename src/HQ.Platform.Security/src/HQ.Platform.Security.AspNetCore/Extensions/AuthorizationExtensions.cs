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

using HQ.Platform.Security.AspNetCore.Requirements;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Security.AspNetCore.Extensions
{
    public static class AuthorizationExtensions
    {
        public static AuthorizationPolicyBuilder RequireAuthenticatedUserExtended(
            this AuthorizationPolicyBuilder builder, IServiceCollection services, SecurityOptions options)
        {
            var serviceProvider = services.BuildServiceProvider();
            var requireAuthenticatedUser = new DenyAnonymousAuthorizationRequirementExtended(serviceProvider, options);
            services.AddSingleton<IAuthorizationHandler>(requireAuthenticatedUser);
            builder.AddRequirements(requireAuthenticatedUser);
            return builder;
        }

        public static AuthorizationPolicyBuilder RequireClaimExtended(this AuthorizationPolicyBuilder builder,
            IServiceCollection services, SecurityOptions security, string claimType, params string[] allowedValues)
        {
            var serviceProvider = services.BuildServiceProvider();
            var requireClaim = new ClaimsAuthorizationRequirementExtended(serviceProvider, security, claimType, allowedValues);
            services.AddSingleton<IAuthorizationHandler>(requireClaim);
            builder.AddRequirements(requireClaim);
            return builder;
        }

        public static AuthorizationPolicyBuilder RequireRoleExtended(this AuthorizationPolicyBuilder builder,
            IServiceCollection services, SecurityOptions options, params string[] allowedRoles)
        {
            var serviceProvider = services.BuildServiceProvider();
            var requireRole = new RolesAuthorizationRequirementExtended(serviceProvider, options, allowedRoles);
            services.AddSingleton<IAuthorizationHandler>(requireRole);
            builder.AddRequirements(requireRole);
            return builder;
        }
    }
}
