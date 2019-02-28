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
using LiteGuard;
using Microsoft.AspNetCore.Authorization;

namespace HQ.Platform.Security.AspNetCore.Requirements
{
    public class RolesAuthorizationRequirementExtended :
        AuthorizationHandler<RolesAuthorizationRequirementExtended>,
        IAuthorizationRequirement
    {
        private readonly SecurityOptions _options;

        public RolesAuthorizationRequirementExtended(SecurityOptions options, IEnumerable<string> allowedRoles)
        {
            Guard.AgainstNullArgument(nameof(options), options);
            Guard.AgainstNullArgument(nameof(allowedRoles), allowedRoles);
            _options = options;
            AllowedRoles = allowedRoles ?? throw new ArgumentNullException(nameof(allowedRoles));
        }

        private bool SupportsSuperUser => _options.SuperUser?.Enabled ?? false;

        public IEnumerable<string> AllowedRoles { get; }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            RolesAuthorizationRequirementExtended requirement)
        {
            if (context.User != null)
            {
                if (SupportsSuperUser && context.User.HasClaim(_options.Claims.RoleClaim, ClaimValues.SuperUser))
                {
                    context.Succeed(requirement);
                }

                var flag = false;
                if (requirement.AllowedRoles != null && requirement.AllowedRoles.Any())
                {
                    flag = requirement.AllowedRoles.Any(r => context.User.IsInRole(r));
                }

                if (flag)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
