using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace snippets.example
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
	        services.AddSingleton(r => HandlerFactory.Default);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var handlers = app.ApplicationServices.GetRequiredService<HandlerFactory>();

            app.Run(async context =>
            {
				// .NET
	            {
					var h = handlers.BuildCSharpHandler(new HandlerInfo());
					var r = (string)h.DynamicInvoke(null, null);
					await context.Response.WriteAsync(r);
				}

	            await context.Response.WriteAsync(Environment.NewLine);

				// Node.js
				{
		            var h = handlers.BuildJavaScriptHandler<string>(new HandlerInfo());
					var r = (string)h.DynamicInvoke(null, null);
		            await context.Response.WriteAsync(r);
	            }
            });
        }
    }
}
