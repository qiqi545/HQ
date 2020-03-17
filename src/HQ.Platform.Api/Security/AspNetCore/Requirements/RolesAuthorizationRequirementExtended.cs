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
using ActiveOptions;
using HQ.Platform.Api.Security.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace HQ.Platform.Api.Security.AspNetCore.Requirements
{
	public class RolesAuthorizationRequirementExtended :
		AuthorizationHandler<RolesAuthorizationRequirementExtended>,
		IAuthorizationRequirement
	{
		private readonly IValidOptionsMonitor<SecurityOptions> _options;
		private readonly IValidOptionsMonitor<SuperUserOptions> _superUser;

		public RolesAuthorizationRequirementExtended(
			IValidOptionsMonitor<SecurityOptions> options,
			IValidOptionsMonitor<SuperUserOptions> superUser,
			IEnumerable<string> allowedRoles)
		{
			_options = options;
			_superUser = superUser;
			AllowedRoles = allowedRoles ?? throw new ArgumentNullException(nameof(allowedRoles));
		}

		private bool SupportsSuperUser => _superUser.CurrentValue?.Enabled ?? false;

		public IEnumerable<string> AllowedRoles { get; }

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
			RolesAuthorizationRequirementExtended requirement)
		{
			var user = context.User;
			if (user != null)
			{
				if (SupportsSuperUser && user.HasClaim(_options.CurrentValue.Claims.RoleClaim, ClaimValues.SuperUser))
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

			return Task.CompletedTask;
		}
	}
}