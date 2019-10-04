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
using HQ.Extensions.Options;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace HQ.Platform.Security.AspNetCore.Requirements
{
	public class DenyAnonymousAuthorizationRequirementExtended :
		AuthorizationHandler<DenyAnonymousAuthorizationRequirementExtended>,
		IAuthorizationRequirement
	{
		private readonly IValidOptionsMonitor<SecurityOptions> _options;
		private readonly IValidOptionsMonitor<SuperUserOptions> _superUser;

		public DenyAnonymousAuthorizationRequirementExtended(
			IValidOptionsMonitor<SecurityOptions> options,
			IValidOptionsMonitor<SuperUserOptions> superUser)
		{
			_options = options;
			_superUser = superUser;
		}

		private bool SupportsSuperUser => _superUser.CurrentValue?.Enabled ?? false;

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
			DenyAnonymousAuthorizationRequirementExtended requirement)
		{
			var user = context.User;

			if (user != null)
			{
				if (SupportsSuperUser && user.HasClaim(_options.CurrentValue.Claims.RoleClaim, ClaimValues.SuperUser))
				{
					context.Succeed(requirement);
				}
				else if ((context.User.Identity == null ? 1 :
					         context.User.Identities.Any(i => i.IsAuthenticated) ? 0 : 1) == 0)
				{
					context.Succeed(requirement);
				}
				else
				{
					context.Fail();
				}
			}

			return Task.CompletedTask;
		}
	}
}