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
using HQ.Cohort.AspNetCore.Mvc.Configuration;
using HQ.Common;
using HQ.Tokens.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Cohort.AspNetCore.Mvc
{
    public static class Use
    {
        public static IApplicationBuilder UseIdentityApi<TUser>(this IApplicationBuilder app,
            Action<IRouteBuilder> configureRoutes = null)
            where TUser : IdentityUser
        {
            Bootstrap.EnsureInitialized();

            app.UseAuthentication();

            var options = app.ApplicationServices.GetService<IOptions<IdentityApiOptions>>();
            if (options?.Value != null)
            {
                var securityOptions = app.ApplicationServices.GetService<IOptions<SecurityOptions>>();

                app.Map(options.Value.RootPath ?? string.Empty, x =>
                {
                    x.UseMvc(routes =>
                    {
                        var tokensEnabled = securityOptions.Value?.Tokens.Enabled ?? false;
                        var identityTypeName = typeof(TUser).Name;

                        if (tokensEnabled)
                        {
                            var tokensPath = securityOptions.Value?.Tokens.Path;
                            if (tokensPath != null)
                            {
                                routes.MapRoute(
                                    "tokens_issue",
                                    tokensPath,
                                    new {controller = $"Token_{identityTypeName}", action = "IssueToken"});

                                routes.MapRoute(
                                    "tokens_verify",
                                    tokensPath,
                                    new {controller = $"Token_{identityTypeName}", action = "VerifyToken"});
                            }
                        }

                        routes.MapRoute("zero_users", "Users/{Action=Index}/{id?}",
                            new {controller = $"Users_{identityTypeName}"});

                        routes.MapRoute("zero_roles", "Roles/{Action=Index}/{id?}",
                            new {controller = $"Roles_{identityTypeName}"});

                        configureRoutes?.Invoke(routes);
                    });
                });
            }

            return app;
        }
    }
}
