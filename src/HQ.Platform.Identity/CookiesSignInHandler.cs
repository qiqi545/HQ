using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ActiveAuth.Events;
using ActiveAuth.Models;
using ActiveTenant;
using HQ.Platform.Api.Security.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Identity
{
	public class CookiesSignInHandler<TTenant, TApplication, TKey> : ISignInHandler 
		where TTenant : IdentityTenant<TKey>
		where TApplication : IdentityApplication<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly IHttpContextAccessor _http;
		private readonly IAuthenticationService _authentication;
		private readonly IOptionsMonitor<SecurityOptions> _securityOptions;

		public CookiesSignInHandler(IHttpContextAccessor http, IAuthenticationService authentication, IOptionsMonitor<SecurityOptions> securityOptions)
		{
			_http = http;
			_authentication = authentication;
			_securityOptions = securityOptions;
		}

		#region Implementation of ISignInHandler

		public async Task OnSignInSuccessAsync(IList<Claim> claims)
		{
			if (_http.HttpContext.GetTenantContext<TTenant>() is TenantContext<TTenant> tenantContext &&
			    tenantContext.Value != null)
			{
				claims.Add(new Claim(_securityOptions.CurrentValue.Claims.TenantIdClaim, $"{tenantContext.Value.Id}"));
				claims.Add(new Claim(_securityOptions.CurrentValue.Claims.TenantNameClaim, tenantContext.Value.Name));
			}

			if (_http.HttpContext.GetApplicationContext<TApplication>() is ApplicationContext<TApplication> applicationContext &&
			    applicationContext.Application != null)
			{
				claims.Add(new Claim(_securityOptions.CurrentValue.Claims.ApplicationIdClaim, $"{applicationContext.Application.Id}"));
				claims.Add(new Claim(_securityOptions.CurrentValue.Claims.ApplicationNameClaim, applicationContext.Application.Name));
			}

			if (_securityOptions.CurrentValue.Cookies.Enabled)
			{
				const string scheme = Common.Constants.Security.Schemes.PlatformCookies;
				var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, scheme));
				var properties = new AuthenticationProperties { IsPersistent = true };
				await _authentication.SignInAsync(_http.HttpContext, scheme, principal, properties);

				_http.HttpContext.User = principal;
			}
		}

		#endregion
	}
}
