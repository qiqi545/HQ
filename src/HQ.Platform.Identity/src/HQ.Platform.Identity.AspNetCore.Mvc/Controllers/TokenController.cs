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
using HQ.Data.Contracts;
using HQ.Data.Contracts.AspNetCore.Mvc;
using HQ.Platform.Api.Attributes;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Api.Extensions;
using HQ.Platform.Api.Models;
using HQ.Platform.Identity.AspNetCore.Mvc.Models;
using HQ.Platform.Identity.Models;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore.Models;
using HQ.Platform.Security.Configuration;
using HQ.Platform.Security.Internal.Extensions;
using ImpromptuInterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Identity.AspNetCore.Mvc.Controllers
{
    [DynamicController]
    [ApiExplorerSettings(IgnoreApi = false)]
    [MetaCategory("Identity", "Manages application access controls.")]
    [DisplayName("Tokens")]
    [MetaDescription("Manages authentication tokens.")]
    public class TokenController<TUser, TTenant, TApplication, TKey> : DataController
        where TUser : IdentityUserExtended<TKey>
        where TTenant : IdentityTenant<TKey>
        where TApplication : IdentityApplication<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly IHttpContextAccessor _http;

        private readonly IOptionsMonitor<PlatformApiOptions> _apiOptions;
        private readonly IOptionsMonitor<SecurityOptions> _securityOptions;
        private readonly ISignInService<TUser, TTenant, TApplication, TKey> _signInService;
        private readonly ILogger<TokenController<TUser, TTenant, TApplication, TKey>> _logger;

        public TokenController(
            IHttpContextAccessor http,
            ISignInService<TUser, TTenant, TApplication, TKey> signInService,
            IOptionsMonitor<SecurityOptions> securityOptions,
            IOptionsMonitor<PlatformApiOptions> apiOptions,
            ILogger<TokenController<TUser, TTenant, TApplication, TKey>> logger)
        {
            _http = http;
            _signInService = signInService;
            _securityOptions = securityOptions;
            _apiOptions = apiOptions;
            _logger = logger;
        }

        [Authorize]
        [HttpPut]
        public IActionResult VerifyToken()
        {
            if (User.Identity == null)
            {
                return Unauthorized();
            }

            if (User.Identity.IsAuthenticated)
            {
                return Ok(User.Claims());
            }

            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> IssueToken([FromBody] BearerTokenRequest model)
        {
            if (!ValidModelState(out var error))
            {
                return error;
            }

            var operation = await _signInService.SignInAsync(model.IdentityType, model.Identity, model.Password, true);
            if (!operation.Succeeded)
            {
                return operation.ToResult();
            }

            Debug.Assert(nameof(IUserIdProvider.Id) == nameof(IdentityUser.Id));

            var user = operation.Data;
            var claims = _http.HttpContext.User.Claims;

            var provider = user.ActLike<IUserIdProvider>();
            var token = AuthenticationExtensions.CreateToken(provider, claims, _securityOptions.CurrentValue, _apiOptions.CurrentValue);

            return Ok(new { AccessToken = token });
        }
    }
}
