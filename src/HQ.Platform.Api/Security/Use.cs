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

using ActiveAuth.Configuration;
using ActiveOptions;
using HQ.Common;
using HQ.Platform.Api.Security.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Api.Security
{
	public static class Use
	{
		public static IApplicationBuilder UseSecurityPolicies(this IApplicationBuilder app)
		{
			var options = app.ApplicationServices.GetRequiredService<IValidOptions<SecurityOptions>>();
			var superUser = app.ApplicationServices.GetRequiredService<IValidOptions<SuperUserOptions>>();

			if (options.Value.Cors.Enabled)
			{
				app.UseRouting();
				app.UseCors(Constants.Security.Policies.CorsPolicy);
				app.UseEndpoints(builder => { });
			}

			if (options.Value.Tokens.Enabled || options.Value.Cookies.Enabled || superUser.Value.Enabled)
			{
				app.UseAuthentication();
				app.UseAuthorization();
			}

			if (options.Value.Https.Enabled)
			{
				app.UseHttpsRedirection();

				return !options.Value.Https.Hsts.Enabled ? app : app.UseHsts();
			}

			return app;
		}
	}
}