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
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts.Mvc;
using HQ.Extensions.Cryptography;
using HQ.Platform.Security.AspNetCore.Models;
using HQ.Platform.Security.AspNetCore.Mvc.Models;
using HQ.Platform.Security.Configuration;
using HQ.Platform.Security.Internal.Extensions;
using ImpromptuInterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Security.AspNetCore.Mvc.Controllers
{
	/// <summary>
	/// A light-weight token issuer that only works against a super user.
	/// </summary>
	[Route("tokens")]
	[DynamicController(typeof(SecurityOptions), nameof(SecurityOptions.SuperUser))]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class SuperUserTokenController : DataController
    {
	    private readonly IOptionsMonitor<SecurityOptions> _securityOptions;

        public SuperUserTokenController(IOptionsMonitor<SecurityOptions> securityOptions)
        {
            _securityOptions = securityOptions;
        }

        [FeatureSelector]
		[AllowAnonymous]
        [HttpPost]
        public Task<IActionResult> IssueToken([FromBody] BearerTokenRequest model,
        [FromHeader(Name = Constants.MultiTenancy.ApplicationHeader)] string application,
        [FromHeader(Name = Constants.MultiTenancy.TenantHeader)] string tenant,
        [FromHeader(Name = Constants.Versioning.VersionHeader)] string version)
		{
			if (!ValidModelState(out var error))
				return Task.FromResult((IActionResult) error);

			bool isSuperUser;
			var superUser = _securityOptions.CurrentValue.SuperUser;
			switch (model.IdentityType)
			{
				case IdentityType.Username:
					isSuperUser = superUser.Username == model.Identity;
					if (!isSuperUser)
						return NotFoundResult();
					break;
				case IdentityType.Email:
					isSuperUser = superUser.Email == model.Identity;
					if (!isSuperUser)
						return NotFoundResult();
					break;
				case IdentityType.PhoneNumber:
					isSuperUser = superUser.PhoneNumber == model.Identity;
					if (!isSuperUser)
						return NotFoundResult();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			
			var encoding = Encoding.UTF8;
			if (Crypto.ConstantTimeEquals(encoding.GetBytes(model.Password), encoding.GetBytes(_securityOptions.CurrentValue.SuperUser.Password)))
			{
				Debug.Assert(nameof(IUserIdProvider.Id) == nameof(IObject.Id));
				var claims = new List<Claim>
				{
					new Claim(_securityOptions?.CurrentValue?.Claims?.RoleClaim ?? ClaimTypes.Role,
						 ClaimValues.SuperUser)
				};
				var provider = new { Id = "87BA0A16-7253-4A6F-A8D4-82DFA1F723C1" }.ActLike<IUserIdProvider>();

				// FIXME: pin claims transformation to user-provided scope
				var token = provider.CreateToken(claims, _securityOptions.CurrentValue);
				return Task.FromResult((IActionResult) Ok(new { AccessToken = token }));
			}

			return UnauthorizedResult();
		}

        [FeatureSelector]
		[DynamicAuthorize(typeof(SecurityOptions), nameof(SecurityOptions.Tokens))]
		[HttpPut]
        public IActionResult VerifyToken()
        {
	        if (!Enabled)
		        return NotFound();

	        if (User.Identity == null)
		        return Unauthorized();

	        if (User.Identity.IsAuthenticated)
				return Ok(new { Data = User.Claims() });

			return Unauthorized();
		}

        private bool Enabled => _securityOptions.CurrentValue.SuperUser.Enabled;
    }
}
