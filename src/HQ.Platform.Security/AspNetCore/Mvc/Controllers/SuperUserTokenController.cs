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
    [DynamicController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SuperUserTokenController : DataController
    {
	    public string ApiName { get; set; } = Assembly.GetExecutingAssembly().GetName()?.Name;
	    public string ApiVersion { get; set; } = Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();

		private readonly IOptions<SecurityOptions> _securityOptions;

        public SuperUserTokenController(IOptions<SecurityOptions> securityOptions)
        {
            _securityOptions = securityOptions;
        }

        [Authorize]
        [HttpPut]
        public IActionResult VerifyToken()
        {
            if (!Debugger.IsAttached)
                return NotImplemented();
#if DEBUG
            if (!_securityOptions.Value.SuperUser.Enabled)
                return NotFound();

            if (User.Identity == null)
                return Unauthorized();

            if (User.Identity.IsAuthenticated)
                return Ok(User.Claims());

            return Unauthorized();
#endif
            return NotImplemented();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> IssueToken([FromBody] BearerTokenRequest model)
        {
            if (!Debugger.IsAttached)
                return NotImplemented();
#if DEBUG
            var superUser = _securityOptions.Value.SuperUser;
            if (!superUser.Enabled)
                return NotFound();

            if (!ValidModelState(out var error))
                return error;

            bool isSuperUser;
            switch (model.IdentityType)
            {
                case IdentityType.Username:
                    isSuperUser = superUser.Username == model.Identity;
                    if (!isSuperUser)
                        return NotFound();
                    break;
                case IdentityType.Email:
                    isSuperUser = superUser.Email == model.Identity;
                    if (!isSuperUser)
                        return NotFound();
                    break;
                case IdentityType.PhoneNumber:
                    isSuperUser = superUser.PhoneNumber == model.Identity;
                    if (!isSuperUser)
                        return NotFound();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var encoding = Encoding.UTF8;
            if (Crypto.ConstantTimeEquals(encoding.GetBytes(model.Password), encoding.GetBytes(_securityOptions.Value.SuperUser.Password)))
            {
                Debug.Assert(nameof(IUserIdProvider.Id) == nameof(IObject.Id));
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, nameof(SecurityOptions.SuperUser))
                };
                var provider = new { Id = "87BA0A16-7253-4A6F-A8D4-82DFA1F723C1" }.ActLike<IUserIdProvider>();
                var token = provider.CreateToken(claims, _securityOptions.Value, ApiVersion, ApiName);
                return Ok(new { AccessToken = token });
            }

            return Unauthorized();
#endif
            return NotImplemented();
        }
    }
}
