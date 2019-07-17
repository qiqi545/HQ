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
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Security.AspNetCore.Requirements
{
    public class ClaimsAuthorizationRequirementExtended :
        AuthorizationHandler<ClaimsAuthorizationRequirementExtended>,
        IAuthorizationRequirement
    {
        private readonly IOptionsMonitor<SecurityOptions> _options;

        public ClaimsAuthorizationRequirementExtended(IOptionsMonitor<SecurityOptions> options, string claimType,
            IEnumerable<string> allowedValues)
        {
            var values = allowedValues.ToArray();

            _options = options;
            ClaimType = claimType;
            AllowedValues = values;
        }

        public ClaimsAuthorizationRequirementExtended(IOptionsMonitor<SecurityOptions> options, string claimType,
            params string[] allowedValues) : this(options, claimType, allowedValues.AsEnumerable())
        {

        }

        private bool SupportsSuperUser => _options.CurrentValue.SuperUser?.Enabled ?? false;

        public string ClaimType { get; }
        public IEnumerable<string> AllowedValues { get; }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ClaimsAuthorizationRequirementExtended requirement)
        {
            var user = context.User;

            if (user != null)
            {
                if (SupportsSuperUser && user.HasClaim(_options.CurrentValue.Claims.RoleClaim, ClaimValues.SuperUser))
                {
                    context.Succeed(requirement);
                }
                else if (requirement.AllowedValues == null || !requirement.AllowedValues.Any()
                    ? user.Claims.Any(c =>
                        string.Equals(c.Type, requirement.ClaimType, StringComparison.OrdinalIgnoreCase))
                    : user.Claims.Any(c =>
                        string.Equals(c.Type, requirement.ClaimType, StringComparison.OrdinalIgnoreCase) &&
                        requirement.AllowedValues.Contains(c.Value, StringComparer.Ordinal)))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
