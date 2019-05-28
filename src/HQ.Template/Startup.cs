#if (DocumentDb)
using HQ.Data.Sql.DocumentDb;
#elif (SqlServer)
using HQ.Data.Sql.SqlServer;
#else
using HQ.Data.Sql.Sqlite;
#endif
using HQ.Extensions.Metrics;
using HQ.Extensions.Metrics.Reporters.AppInsights;
using HQ.Installer;
using HQ.Platform.Security.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Swagger;
#if AppInsights
using Microsoft.ApplicationInsights.AspNetCore;
#endif

namespace HQ.Template
{
    public class Startup
    {
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
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var dbConfig = _configuration.GetSection("DbOptions");
            var config = _configuration.GetSection("HQ");

            services.AddHq(_environment, config);

#if DocumentDb
            services.AddBackendServices(DatabaseType.DocumentDb, connectionString, dbConfig, config)
                .AddGenerated<SqliteOptions>(config.GetSection("Security"), "/api");
#elif SqlServer
            services.AddBackendServices(DatabaseType.SqlServer, connectionString, dbConfig, config)
                .AddGenerated<SqliteOptions>(config.GetSection("Security"), "/api");
#else
            services.AddBackendServices(DatabaseType.Sqlite, connectionString, dbConfig, config)
                .AddGenerated<SqliteOptions>(config.GetSection("Security"), "/api");
#endif

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

            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.SwaggerDoc("swagger", new Info
                {
                    Title = "Sample API",
                    Version = "v1"
                });
                c.DescribeAllEnumsAsStrings();
            });

            services.AddAdminUi();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/meta/swagger", "Swagger 2.0");
                c.RoutePrefix = "docs/swagger";
            });

            app.UseAdminUi();
        }
    }
}
