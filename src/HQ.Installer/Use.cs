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
using Blowdart.UI;
using Blowdart.UI.Web;
using Blowdart.UI.Web.SemanticUI;
using HQ.Common;
using HQ.Platform.Api;
using HQ.Platform.Identity.Models;
using HQ.Platform.Operations;
using HQ.Platform.Security.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Installer
{
    public static class Use
    {
        public static IApplicationBuilder UseHq(this IApplicationBuilder app,
            Action<IRouteBuilder> configureRoutes = null)
        {
            Bootstrap.EnsureInitialized();

            app.UseSecurityPolicies();
            app.UseOperationsApi();
            app.UsePlatformApi();
            app.UseMultiTenancy<IdentityTenant, string>();
            app.UseVersioning();

            app.UseMvc(routes =>
            {
                configureRoutes?.Invoke(routes);

                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            return app;
        }

        public static IApplicationBuilder UseAdminUi(this IApplicationBuilder app)
        {
            // TODO: this is a workaround for not being able to detect a UiSystem declared on an inner component
            var settings = app.ApplicationServices.GetRequiredService<UiSettings>();
            settings.DefaultSystem = new SemanticUi();
            app.UseBlowdartUi(root => { });
            return app;
        }
    }
}
