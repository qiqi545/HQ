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

#if (DocumentDb)
using HQ.Data.Sql.DocumentDb;
#elif (SqlServer)
using HQ.Data.Sql.SqlServer;
#else
using HQ.Data.Sql.Sqlite;
#endif

using HQ.Extensions.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

#if AppInsights
using HQ.Extensions.Metrics.Reporters.AppInsights;
using Constants = HQ.Common.Constants;
#endif

namespace HQ.Template
{
    public class Startup
    {
        public static void Main(string[] args) => HqServer.Start<Startup>(args);

        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHq(_environment, (IConfigurationRoot)_configuration);

#if DocumentDb
            const string dbType = "DocumentDb";
#elif SqlServer
            const string dbType = "SqlServer";
#else
            const string dbType = "Sqlite";
#endif
            services.AddBackendServices(dbType, _configuration.GetConnectionString("DefaultConnection"), _configuration.GetSection("DbOptions"), _configuration.GetSection("HQ"));

            //services.AddGenerated<SqliteOptions>(_configuration.GetSection("HQ").GetSection("Security"))

#if AppInsights
            services.AddApplicationInsightsTelemetry(_configuration.GetSection("Azure").GetSection("ApplicationInsights"));
#endif

            services.AddMetrics(o =>
            {
#if AppInsights
                o.PushToApplicationInsights(p =>
                {
                    p.MetricsSampleEventName = Common.Constants.Events.MetricsSample;
                    p.HealthCheckEventName = Common.Constants.Events.HealthCheck;
                    p.PublishHealthChecks = true;
                    p.PublishHealthy = false;
                    p.PublishMetrics = true;
                });
#endif
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHq();
        }
    }
}
