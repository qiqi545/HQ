#if (DocumentDb)
using HQ.Data.Sql.DocumentDb;
#elif (SqlServer)
using HQ.Data.Sql.SqlServer;
#else
using HQ.Data.Sql.Sqlite;
#endif
using HQ.Installer;
using HQ.Platform.Security.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
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

        public void ConfigureServices(IServiceCollection services)
        {
#if DocumentDb
            services.AddHq(_environment,DatabaseType.DocumentDb, _configuration.GetSection("HQ"))
                .AddSecurityPolicies(_configuration.GetSection("HQ").GetSection("Security"))
                .AddGenerated<DocumentDbBatchOptions>(_configuration.GetSection("HQ").GetSection("Security"), "/api")
#elif SqlServer
            services.AddHq(_environment,DatabaseType.SqlServer, _configuration.GetSection("HQ"))
                .AddSecurityPolicies(_configuration.GetSection("HQ").GetSection("Security"))
                .AddGenerated<SqlServerOptions>(_configuration.GetSection("HQ").GetSection("Security"), "/api")
#else
            services.AddHq(_environment, DatabaseType.Sqlite, _configuration.GetSection("HQ"))
                .AddSecurityPolicies(_configuration.GetSection("HQ").GetSection("Security"))
                .AddGenerated<SqliteOptions>(_configuration.GetSection("HQ").GetSection("Security"), "/api")
#endif

#if AppInsights
                .AddApplicationInsightsTelemetry(_configuration.GetSection("HQ").GetSection("AppInsights"))
#endif

                .AddUi();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHq();
        }
    }
}
