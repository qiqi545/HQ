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

using HQ.Tokens.Configuration;
using HQ.Tokens.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Tokens.Extensions
{
    public static class AuthorizationExtensions
    {
        public static AuthorizationPolicyBuilder RequireAuthenticatedUserExtended(
            this AuthorizationPolicyBuilder builder, IServiceCollection services, SecurityOptions options)
        {
            var requireAuthenticatedUser = new DenyAnonymousAuthorizationRequirementExtended(options);
            services.AddSingleton<IAuthorizationHandler>(requireAuthenticatedUser);
            builder.AddRequirements(requireAuthenticatedUser);
            return builder;
        }

        public static AuthorizationPolicyBuilder RequireClaimExtended(this AuthorizationPolicyBuilder builder,
            IServiceCollection services, SecurityOptions security, string claimType, params string[] allowedValues)
        {
            var requireClaim = new ClaimsAuthorizationRequirementExtended(security, claimType, allowedValues);
            services.AddSingleton<IAuthorizationHandler>(requireClaim);
            builder.AddRequirements(requireClaim);
            return builder;
        }

        public static AuthorizationPolicyBuilder RequireRoleExtended(this AuthorizationPolicyBuilder builder,
            IServiceCollection services, SecurityOptions options, params string[] allowedRoles)
        {
            var requireRole = new RolesAuthorizationRequirementExtended(options, allowedRoles);
            services.AddSingleton<IAuthorizationHandler>(requireRole);
            builder.AddRequirements(requireRole);
            return builder;
        }
    }
}
