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
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.AspNetCore.Mvc;
using HQ.Data.Contracts.Attributes;
using HQ.Platform.Api.Attributes;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Api.Extensions;
using HQ.Platform.Api.Models;
using HQ.Platform.Identity.AspNetCore.Mvc.Models;
using HQ.Platform.Identity.Extensions;
using HQ.Platform.Identity.Models;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore.Models;
using HQ.Platform.Security.Configuration;
using HQ.Platform.Security.Internal.Extensions;
using ImpromptuInterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Identity.AspNetCore.Mvc.Controllers
{
    [DynamicController]
    [ApiExplorerSettings(IgnoreApi = false)]
    [MetaCategory("Identity", "Manages application access controls."), DisplayName("Tokens"), MetaDescription("Manages authentication tokens.")]
    public class TokenController<TUser, TTenant, TKey> : DataController
        where TUser : IdentityUserExtended<TKey>
        where TTenant : IdentityTenant
        where TKey : IEquatable<TKey>
    {
        private readonly IOptions<PlatformApiOptions> _apiOptions;
        private readonly ILogger<TokenController<TUser, TTenant, TKey>> _logger;
        private readonly IOptions<SecurityOptions> _securityOptions;
        private readonly IServerTimestampService _timestamps;
        private readonly UserManager<TUser> _userManager;

        public TokenController(
            UserManager<TUser> userManager,
            IServerTimestampService timestamps,
            IOptions<SecurityOptions> securityOptions,
            IOptions<PlatformApiOptions> apiOptions,
            ILogger<TokenController<TUser, TTenant, TKey>> logger)
        {
            _userManager = userManager;
            _timestamps = timestamps;
            _securityOptions = securityOptions;
            _apiOptions = apiOptions;
            _logger = logger;
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
            if (!ValidModelState(out var error))
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
                Debug.Assert(nameof(IUserIdProvider.Id) == nameof(IdentityUser.Id));

                var claims = await _userManager.GetClaimsAsync(user);

                if (HttpContext.GetTenantContext<TTenant>() is TenantContext<TTenant> tenantContext)
                {
                    if (tenantContext.Tenant != null)
                    {
                        claims.Add(new Claim(_securityOptions.Value.Claims.TenantIdClaim, tenantContext.Tenant.Id));
                        claims.Add(new Claim(_securityOptions.Value.Claims.TenantNameClaim, tenantContext.Tenant.Name));
                    }
                }

                var provider = user.ActLike<IUserIdProvider>();
                var token = JwtSecurity.CreateToken(provider, claims, _securityOptions.Value, _apiOptions.Value);

                return Ok(new
                {
                    AccessToken = token
                });
            }

            return Unauthorized();
        }
    }
}
