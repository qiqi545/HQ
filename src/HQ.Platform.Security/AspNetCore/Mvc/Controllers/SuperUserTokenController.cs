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
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
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
	/// While this controller is useful for setting up environments that don't use platform identity services,
	/// it should not be used, and does not work, in a non-debugging context. 
	/// </summary>
	[Route("tokens")]
	[DynamicController]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class SuperUserTokenController : DataController
    {
	    public string ApiName { get; set; } = Assembly.GetExecutingAssembly().GetName().Name;
	    public string ApiVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

		private readonly IOptionsMonitor<SecurityOptions> _securityOptions;

        public SuperUserTokenController(IOptionsMonitor<SecurityOptions> securityOptions)
        {
            _securityOptions = securityOptions;
        }

        [AllowAnonymous]
        [HttpPost]
        public Task<IActionResult> IssueToken([FromBody] BearerTokenRequest model)
        {
#if DEBUG
            if (!Enabled)
                return NotFoundResult();

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
                var token = provider.CreateToken(claims, _securityOptions.CurrentValue, ApiVersion, ApiName);
                return Task.FromResult((IActionResult) Ok(new { AccessToken = token }));
            }

            return UnauthorizedResult();
#else
			return NotImplementedResult();
#endif
		}

		[DynamicAuthorize(typeof(SecurityOptions), nameof(SecurityOptions.Tokens))]
		[HttpPut]
        public IActionResult VerifyToken()
        {
#if DEBUG
	        if (!Enabled)
		        return NotFound();

	        if (User.Identity == null)
		        return Unauthorized();

	        if (User.Identity.IsAuthenticated)
		        return Ok(User.Claims());

	        return Unauthorized();
#else
			return NotImplemented();
#endif
        }

        private bool Enabled => _securityOptions.CurrentValue.SuperUser.Enabled;
    }
}
