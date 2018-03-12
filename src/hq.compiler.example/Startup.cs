using System.Reflection;
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
                MethodInfo h = handlers.BuildHandler(new HandlerInfo());

                var r = (string)h.Invoke(null, new object[] { });

                await context.Response.WriteAsync(r);
            });
        }
    }
}
