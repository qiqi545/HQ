using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace HQ.Common.AspNetCore
{
	public static class Use
	{
		public static IApplicationBuilder UseMvcWithDefaultRoute(this IApplicationBuilder app, Action<IRouteBuilder> configureRoutes)
		{
			return app.UseMvc(routes =>
			{
				configureRoutes?.Invoke(routes);

				routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
