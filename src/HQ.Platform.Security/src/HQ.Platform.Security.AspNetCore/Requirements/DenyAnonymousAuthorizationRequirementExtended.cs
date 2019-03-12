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

using System.Linq;
using System.Threading.Tasks;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace HQ.Platform.Security.AspNetCore.Requirements
{
    public class DenyAnonymousAuthorizationRequirementExtended :
        AuthorizationHandler<DenyAnonymousAuthorizationRequirementExtended>,
        IAuthorizationRequirement
    {
        private readonly SecurityOptions _options;

        public DenyAnonymousAuthorizationRequirementExtended(SecurityOptions options)
        {
            _options = options;
        }

        private bool SupportsSuperUser => _options.SuperUser?.Enabled ?? false;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            DenyAnonymousAuthorizationRequirementExtended requirement)
        {
            if (context.User != null)
            {
                if (SupportsSuperUser && context.User.HasClaim(_options.Claims.RoleClaim, ClaimValues.SuperUser))
                {
                    context.Succeed(requirement);
                }
                else if ((context.User.Identity == null ? 1 :
                             context.User.Identities.Any(i => i.IsAuthenticated) ? 0 : 1) == 0)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
