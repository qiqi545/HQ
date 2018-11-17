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
using System.Diagnostics;
using System.Threading.Tasks;
using HQ.Cohort.AspNetCore.Mvc.Models;
using HQ.Cohort.Extensions;
using HQ.Common.AspNetCore;
using HQ.Common.Models;
using HQ.Rosetta.Queryable;
using HQ.Tokens;
using HQ.Tokens.Configuration;
using HQ.Tokens.Extensions;
using ImpromptuInterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HQ.Cohort.AspNetCore.Mvc.Controllers
{
    [Route("tokens")]
    [DynamicController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TokenController<TUser> : ControllerExtended where TUser : IdentityUser
    {
        private readonly IOptions<SecurityOptions> _securityOptions;
        private readonly IServerTimestampService _timestamps;
        private readonly UserManager<TUser> _userManager;

        public TokenController(
            UserManager<TUser> userManager,
            IServerTimestampService timestamps,
            IOptions<SecurityOptions> securityOptions,
            IQueryableProvider<TUser> queryable,
            ILogger<TokenController<TUser>> logger)
        {
            _userManager = userManager;
            _timestamps = timestamps;
            _securityOptions = securityOptions;
        }

        [Authorize]
        [HttpPut]
        public IActionResult VerifyToken()
        {
            if (User.Identity == null)
                return Unauthorized();

            if (User.Identity.IsAuthenticated)
                return Ok(User.GetClaims());

            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> IssueToken([FromBody] BearerTokenRequest model)
        {
            if (!TryValidateModelState(out var error))
                return error;

            TUser user;
            switch (model.IdentityType)
            {
                case IdentityType.Username:
                    user = await _userManager.FindByNameAsync(model.Identity);
                    break;
                case IdentityType.Email:
                    user = await _userManager.FindByEmailAsync(model.Identity);
                    if (!user.EmailConfirmed)
                        return NotFound();
                    break;
                case IdentityType.PhoneNumber:
                    user = await _userManager.FindByPhoneNumberAsync(model.Identity);
                    if (!user.PhoneNumberConfirmed)
                        return NotFound();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (user == null)
                return NotFound();

            if (user.LockoutEnd.HasValue && user.LockoutEnd > _timestamps.GetCurrentTime())
                return Forbid();

            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                Debug.Assert(nameof(IUserNameProvider.UserName) == nameof(IdentityUser.UserName));

                return Ok(new
                {
                    AccessToken = JwtSecurity.CreateToken(user.ActLike<IUserNameProvider>(),
                        await _userManager.GetClaimsAsync(user), _securityOptions.Value)
                });
            }

            return Unauthorized();
        }
    }
}
