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
using System.Threading.Tasks;
using ActiveAuth.Models;
using ActiveErrors;
using ActiveLogging;
using ActiveRoutes;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts.Mvc;
using HQ.Platform.Api.Security;
using HQ.Platform.Api.Security.AspNetCore.Models;
using HQ.Platform.Api.Security.AspNetCore.Mvc.Models;
using HQ.Platform.Api.Security.Configuration;
using HQ.Platform.Api.Security.Internal.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.Platform.Identity.Controllers
{
	[Route("tokens")]
	[DynamicController(typeof(SecurityOptions), new []{nameof(SecurityOptions.Tokens)})]
	[ApiExplorerSettings(IgnoreApi = false)]
	[MetaCategory("Authentication", "Manages authenticating incoming users against policies and identities, if any.")]
	[DisplayName("Tokens")]
	[MetaDescription("Manages authentication tokens.")]
	public class TokenController<TUser, TTenant, TApplication, TKey> : Controller,
		IDynamicComponentEnabled<TokensApiFeature>
		where TUser : IdentityUserExtended<TKey>
		where TTenant : IdentityTenant<TKey>
		where TApplication : IdentityApplication<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly IHttpContextAccessor _http;
		private readonly ISafeLogger<TokenController<TUser, TTenant, TApplication, TKey>> _logger;

		private readonly IOptionsMonitor<SecurityOptions> _securityOptions;
		private readonly ISignInService<TUser> _signInService;

		public TokenController(
			IHttpContextAccessor http,
			ISignInService<TUser> signInService,
			IOptionsMonitor<SecurityOptions> securityOptions,
			ISafeLogger<TokenController<TUser, TTenant, TApplication, TKey>> logger)
		{
			_http = http;
			_signInService = signInService;
			_securityOptions = securityOptions;
			_logger = logger;
		}

		[DynamicAuthorize(typeof(SecurityOptions), nameof(SecurityOptions.Tokens))]
		[DynamicHttpPut]
		public IActionResult VerifyToken()
		{
			if (User.Identity == null)
			{
				_logger.Trace(() => "User is unauthorized");

				return Unauthorized();
			}

			if (User.Identity.IsAuthenticated)
			{
				_logger.Trace(() => "{User} verified token", User.Identity.Name);

				return Ok(User.Claims());
			}

			return Unauthorized();
		}

		// FIXME: defaults for headers should come from a dynamic source

		[AllowAnonymous]
		[DynamicHttpPost]
		public async Task<IActionResult> IssueToken(
			[FromBody] BearerTokenRequest model,
			[FromHeader(Name = ActiveTenant.Constants.MultiTenancy.ApplicationHeader)]
			string application,
			[FromHeader(Name = ActiveTenant.Constants.MultiTenancy.TenantHeader)]
			string tenant,
			[FromHeader(Name = ActiveVersion.Constants.Versioning.VersionHeader)]
			string version
		)
		{
			if (!this.TryValidateModelOrError(ModelState, ErrorEvents.ValidationFailed, HQ.Data.Contracts.ErrorStrings.ValidationFailed, out var error))
				return error;

			// FIXME: pin claims transformation to user-provided scope
			var operation = await _signInService.SignInAsync(model.IdentityType, model.Identity, model.Password);
			if (!operation.Succeeded)
				return operation.ToResult();

			Debug.Assert(nameof(IUserIdProvider<TKey>.Id) == nameof(IdentityUser.Id));

			var user = operation.Data;
			var claims = _http.HttpContext.User.Claims;

			var identity = user.QuackLike<IUserIdProvider<TKey>>();
			var timestamps = Request.HttpContext.RequestServices.GetRequiredService<IServerTimestampService>();
			var token = identity.CreateToken<IUserIdProvider<TKey>, TKey>(timestamps,
				claims, _securityOptions.CurrentValue);

			return Ok(new {AccessToken = token});
		}
	}
}