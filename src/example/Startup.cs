using System.Collections.Generic;
using System.Reflection;
using hq.compiler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Example
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IAssemblyLoadContextProvider>(r => new AssemblyLoadContextProvider());
            services.AddSingleton<IMetadataReferenceResolver>(r => new DefaultMetadataReferenceResolver());
            services.AddSingleton<IAssemblyBuilder>(r => new DefaultAssemblyBuilder(r.GetRequiredService<IAssemblyLoadContextProvider>(), r.GetRequiredService<IEnumerable<IMetadataReferenceResolver>>()));
            services.AddSingleton(r => new HandlerFactory(r.GetRequiredService<IAssemblyBuilder>()));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

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
