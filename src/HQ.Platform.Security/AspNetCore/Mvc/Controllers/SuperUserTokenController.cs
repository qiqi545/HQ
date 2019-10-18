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
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Common.AspNetCore;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts;
using HQ.Data.Contracts.AspNetCore.Attributes;
using HQ.Data.Contracts.AspNetCore.Mvc;
using HQ.Data.Contracts.Attributes;
using HQ.Extensions.Cryptography;
using HQ.Extensions.Options;
using HQ.Platform.Security.AspNetCore.Models;
using HQ.Platform.Security.AspNetCore.Mvc.Configuration;
using HQ.Platform.Security.AspNetCore.Mvc.Models;
using HQ.Platform.Security.Configuration;
using HQ.Platform.Security.Internal.Extensions;
using ImpromptuInterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Security.AspNetCore.Mvc.Controllers
{
	/// <summary>
	///     A light-weight token issuer that only works against a super user.
	/// </summary>
	[Route("tokens")]
	[DynamicController(typeof(SuperUserOptions))]
	[ApiExplorerSettings(IgnoreApi = false)]
	[MetaCategory("Authentication", "Manages authenticating incoming users against policies and identities, if any.")]
	[DisplayName("Tokens")]
	[MetaDescription("Manages authentication tokens.")]
	public class SuperUserTokenController<TKey> : DataController, IDynamicComponentEnabled<SuperUserComponent> where TKey : IEquatable<TKey>
	{
		private readonly IValidOptionsSnapshot<SuperUserOptions> _options;
		private readonly IValidOptionsSnapshot<SecurityOptions> _security;

		public SuperUserTokenController(IValidOptionsSnapshot<SuperUserOptions> options,
			IValidOptionsSnapshot<SecurityOptions> security)
		{
			_options = options;
			_security = security;
		}

		private bool Enabled => _options.Value.Enabled;

		[FeatureSelector]
		[AllowAnonymous]
		[HttpPost]
		public Task<IActionResult> IssueToken([FromBody] BearerTokenRequest model,
			[FromHeader(Name = Constants.MultiTenancy.ApplicationHeader)]
			string application,
			[FromHeader(Name = Constants.MultiTenancy.TenantHeader)]
			string tenant,
			[FromHeader(Name = Constants.Versioning.VersionHeader)]
			string version)
		{
			if (!ValidModelState(out var error))
				return Task.FromResult((IActionResult) error);

			bool isSuperUser;
			var superUser = _options.Value;
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
			if (Crypto.ConstantTimeEquals(encoding.GetBytes(model.Password), encoding.GetBytes(_options.Value.Password)))
			{
				Debug.Assert(nameof(IUserIdProvider<string>.Id) == nameof(IObject.Id));
				var claims = new List<Claim>
				{
					new Claim(_security.Value?.Claims?.RoleClaim ?? ClaimTypes.Role, ClaimValues.SuperUser)
				};

				var provider = new {Id = "87BA0A16-7253-4A6F-A8D4-82DFA1F723C1"}
					.ActLike<IUserIdProvider<TKey>>();

				// FIXME: pin claims transformation to user-provided scope
				var timestamps = Request.HttpContext.RequestServices.GetRequiredService<IServerTimestampService>();
				var token = provider.CreateToken<IUserIdProvider<TKey>, TKey>(timestamps, claims, _security.Value);
				return Task.FromResult((IActionResult) Ok(new {AccessToken = token}));
			}
			
			return UnauthorizedResult();
		}

		[FeatureSelector]
		[DynamicAuthorize(typeof(SuperUserOptions))]
		[HttpPut]
		public IActionResult VerifyToken()
		{
			if (!Enabled)
				return NotFound();

			if (User.Identity == null)
				return Unauthorized(new { Message = "Super user identity not found. "});

			if (User.Identity.IsAuthenticated)
				return Ok(new {Data = User.Claims()});

			return Unauthorized(new { Message = "Super user identity not authenticated. " });
		}
	}
}