using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace HQ.Template
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHq(DatabaseType.CosmosDb, _configuration.GetSection("HQ"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHq();
        }
    }
}
