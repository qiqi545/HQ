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
using System.Security.Claims;
using System.Threading.Tasks;
using ActiveErrors;
using ActiveTenant;
using HQ.Common;
using HQ.Data.Contracts;
using HQ.Platform.Api.Extensions;
using HQ.Platform.Api.Models;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Extensions;
using HQ.Platform.Identity.Models;
using HQ.Platform.Security.AspNetCore.Mvc.Models;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Constants = HQ.Common.Constants;
using HttpContextExtensions = ActiveTenant.HttpContextExtensions;

namespace HQ.Platform.Identity.Services
{
	public class SignInService<TUser, TTenant, TApplication, TKey> : ISignInService<TUser, TTenant, TApplication, TKey>
		where TUser : IdentityUserExtended<TKey>, IUserEmailProvider, IPhoneNumberProvider
		where TTenant : IdentityTenant<TKey>
		where TApplication : IdentityApplication<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly IAuthenticationService _authentication;

		private readonly IHttpContextAccessor _http;
		private readonly IOptionsMonitor<IdentityOptionsExtended> _identityOptions;
		private readonly IOptionsMonitor<SecurityOptions> _securityOptions;
		private readonly SignInManager<TUser> _signInManager;
		private readonly UserManager<TUser> _userManager;

		public SignInService(IHttpContextAccessor http,
			UserManager<TUser> userManager,
			SignInManager<TUser> signInManager,
			IAuthenticationService authentication,
			IOptionsMonitor<IdentityOptionsExtended> identityOptions,
			IOptionsMonitor<SecurityOptions> securityOptions)
		{
			_http = http;
			_userManager = userManager;
			_signInManager = signInManager;
			_authentication = authentication;
			_identityOptions = identityOptions;
			_securityOptions = securityOptions;
		}

		public async Task<Operation<TUser>> SignInAsync(IdentityType identityType, string identity, string password,
			bool isPersistent)
		{
			TUser user = default;
			try
			{
				user = await _userManager.FindByIdentityAsync(identityType, identity);
				if (user == null)
					return IdentityResultExtensions.NotFound<TUser>();

				var result = await _signInManager.CheckPasswordSignInAsync(user, password,
					_identityOptions.CurrentValue.User.LockoutEnabled);

				if (result.Succeeded)
				{
					var claims = await BuildClaimsAsync(user, _http.HttpContext);

					if (_securityOptions.CurrentValue.Cookies.Enabled)
					{
						const string scheme = Constants.Security.Schemes.PlatformCookies;
						var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, scheme));
						var properties = new AuthenticationProperties {IsPersistent = isPersistent};
						await _authentication.SignInAsync(_http.HttpContext, scheme, principal, properties);

						_http.HttpContext.User = principal;
					}
				}

				return result.ToOperation(user);
			}
			catch (Exception ex)
			{
				var operation = IdentityResult.Failed(new IdentityError
				{
					Code = ex.GetType().Name, Description = ex.Message
				}).ToOperation(user);

				return operation;
			}
		}

		public async Task<Operation> SignOutAsync(TUser user)
		{
			try
			{
				await _signInManager.SignOutAsync();
				return Operation.CompletedWithoutErrors;
			}
			catch (Exception ex)
			{
				var operation = IdentityResult.Failed(new IdentityError
				{
					Code = ex.GetType().Name, Description = ex.Message
				}).ToOperation();

				return operation;
			}
		}

		private async Task<IList<Claim>> BuildClaimsAsync(TUser user, HttpContext context)
		{
			var claims = await _userManager.GetClaimsAsync(user);

			if (context.GetTenantContext<TTenant>() is TenantContext<TTenant> tenantContext &&
			    tenantContext.Value != null)
			{
				claims.Add(new Claim(_securityOptions.CurrentValue.Claims.TenantIdClaim, $"{tenantContext.Value.Id}"));
				claims.Add(new Claim(_securityOptions.CurrentValue.Claims.TenantNameClaim, tenantContext.Value.Name));
			}

			if (HttpContextExtensions.GetApplicationContext<TApplication>(context) is ApplicationContext<TApplication> applicationContext &&
			    applicationContext.Application != null)
			{
				claims.Add(new Claim(_securityOptions.CurrentValue.Claims.ApplicationIdClaim,
					$"{applicationContext.Application.Id}"));
				claims.Add(new Claim(_securityOptions.CurrentValue.Claims.ApplicationNameClaim,
					applicationContext.Application.Name));
			}

			return claims;
		}
	}
}