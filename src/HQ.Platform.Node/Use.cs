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
using HQ.Common;
using HQ.Extensions.Logging;
using HQ.Platform.Api;
using HQ.Platform.Api.Correlation;
using HQ.Platform.Identity.Models;
using HQ.Platform.Operations;
using HQ.Platform.Security.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;

#if NETCOREAPP2_2
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
#else
using Microsoft.Extensions.Hosting;
#endif

namespace HQ.Platform.Node
{
    public static class Use
    {
        public static IApplicationBuilder UseHq(this IApplicationBuilder app, IWebHostEnvironment env, ISafeLogger logger = null,
#if NETCOREAPP2_2
			Action<IRouteBuilder> configureRoutes = null
#else
			Action<IEndpointRouteBuilder> configureRoutes = null
#endif
			)
		{
            Bootstrap.EnsureInitialized();

            if (env.IsDevelopment())
	            app.UseDeveloperExceptionPage();

			app.UseTraceContext();

#if NETCOREAPP2_2
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/meta/swagger", "Swagger 2.0");
				c.RoutePrefix = "docs/swagger";
			});
#endif

			app.UseSecurityPolicies();
            app.UseVersioning();
			app.UseOperationsApi();
            app.UsePlatformApi();
            app.UseConfigurationApi();
            app.UseMetaApi();
            app.UseMultiTenancy<IdentityTenant, string>();

#if NETCOREAPP2_2
			app.UseStaticFiles();
			app.UseMvc(routes =>
	        {
		        try
		        {
			        configureRoutes?.Invoke(routes);
		        }
		        catch (Exception e)
		        {
			        logger?.Critical(() => "Error encountered when starting MVC for HQ services", e);
			        throw;
		        }
	        });
#else
			app.UseStaticFiles();
			app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                try
	            {
		            configureRoutes?.Invoke(endpoints);
					endpoints.MapControllers();
				}
	            catch (Exception e)
	            {
		            logger?.Critical(() => "Error encountered when starting MVC for HQ services", e);
		            throw;
	            }
            });
#endif
			return app;
        }
    }
}
