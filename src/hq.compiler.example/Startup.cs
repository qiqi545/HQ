using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace hq.compiler.example
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
                var h = handlers.BuildHandler(new HandlerInfo());
	            var r = (string) h.DynamicInvoke(null, null);
                await context.Response.WriteAsync(r);
            });
        }
    }
}
