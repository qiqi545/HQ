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
using System.Linq;
using System.Threading.Tasks;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Platform.Security.AspNetCore.Requirements
{
    public class RolesAuthorizationRequirementExtended :
        AuthorizationHandler<RolesAuthorizationRequirementExtended>,
        IAuthorizationRequirement
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SecurityOptions _options;

        public RolesAuthorizationRequirementExtended(IServiceProvider serviceProvider, SecurityOptions options, IEnumerable<string> allowedRoles)
        {
            _serviceProvider = serviceProvider;
            _options = options;
            AllowedRoles = allowedRoles ?? throw new ArgumentNullException(nameof(allowedRoles));
        }

        private bool SupportsSuperUser => _options.SuperUser?.Enabled ?? false;

        public IEnumerable<string> AllowedRoles { get; }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            RolesAuthorizationRequirementExtended requirement)
        {
            var user = context.User;

            //
            // ASP.NET Core Identity is calling the requirement *before* validating and injecting the claim!
            //
            if (!user.Identity.IsAuthenticated)
            {
                if (_serviceProvider.GetService(typeof(IAuthenticationService)) is IAuthenticationService service && context.Resource is AuthorizationFilterContext filter)
                {
                    var result = await service.AuthenticateAsync(filter.HttpContext, JwtBearerDefaults.AuthenticationScheme);
                    if (result.Succeeded)
                    {
                        user = result.Principal;
                    }
                }
            }

            if (user != null)
            {
                if (SupportsSuperUser && user.HasClaim(_options.Claims.RoleClaim, ClaimValues.SuperUser))
                {
                    context.Succeed(requirement);
                }

                var flag = false;
                if (requirement.AllowedRoles != null && requirement.AllowedRoles.Any())
                {
                    flag = requirement.AllowedRoles.Any(r => user.IsInRole(r));
                }

                if (flag)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
