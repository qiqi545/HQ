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

using ActiveAuth.Providers;
using HQ.Platform.Api.Security.Configuration;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Api
{
	internal sealed class AuthBridge : IIdentityClaimNameProvider, ICookiesInfoProvider
	{
		private readonly IOptionsSnapshot<SecurityOptions> _options;

		public AuthBridge(IOptionsSnapshot<SecurityOptions> options)
		{
			_options = options;
		}

		#region Implementation of IIdentityClaimNameProvider

		public string TenantIdClaim => _options.Value.Claims.TenantIdClaim;
		public string TenantNameClaim => _options.Value.Claims.TenantNameClaim;
		public string ApplicationIdClaim => _options.Value.Claims.ApplicationIdClaim;
		public string ApplicationNameClaim => _options.Value.Claims.ApplicationNameClaim;
		public string UserIdClaim => _options.Value.Claims.UserIdClaim;
		public string UserNameClaim => _options.Value.Claims.UserNameClaim;
		public string RoleClaim => _options.Value.Claims.RoleClaim;
		public string EmailClaim => _options.Value.Claims.EmailClaim;
		public string PermissionClaim => _options.Value.Claims.PermissionClaim;

		#endregion

		#region Implementation of ICookiesInfoProvider

		public bool Enabled => _options.Value.Cookies.Enabled;
		public string Scheme => _options.Value.Cookies.Scheme;

		#endregion
	}
}