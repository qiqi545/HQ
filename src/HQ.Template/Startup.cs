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

using HQ.Extensions.Deployment;
using HQ.Extensions.Deployment.Azure;
using HQ.Extensions.Logging;
using HQ.Platform.Node;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace HQ.Template
{
    public class Startup
    {
        public static void Main(string[] args) => Server.Start<Startup>(args, GetCloudOptions);

        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;
        private readonly ISafeLogger<Startup> _logger;

        public Startup(IConfiguration configuration, IHostingEnvironment environment, ISafeLogger<Startup> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHq(_environment, _configuration, _logger, mvc => { /* custom MVC configuration */ }, GetCloudOptions(_configuration));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHq(_logger, routes => { /* custom MVC routes */});
        }

        private static ICloudOptions[] GetCloudOptions(IConfiguration config)
        {
            return new ICloudOptions[] { config.GetSection("Cloud").Get<AzureOptions>() };
        }
    }
}
