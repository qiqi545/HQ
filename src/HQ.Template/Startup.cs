#if (DocumentDb)
using HQ.Data.Sql.DocumentDb;
#elif (SqlServer)
using HQ.Data.Sql.SqlServer;
#else
using HQ.Data.Sql.Sqlite;
#endif
using HQ.Installer;
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
#if DocumentDb
            services.AddHq(DatabaseType.DocumentDb, _configuration.GetSection("HQ"))
                .AddAuthorizationPolicies(_configuration.GetSection("HQ").GetSection("Security"))
                .AddGenerated<DocumentDbBatchOptions>(_configuration.GetSection("HQ").GetSection("Security"), "/api")
#elif SqlServer
            services.AddHq(DatabaseType.SqlServer, _configuration.GetSection("HQ"))
                .AddAuthorizationPolicies(_configuration.GetSection("HQ").GetSection("Security"))
                .AddGenerated<SqlServerOptions>(_configuration.GetSection("HQ").GetSection("Security"), "/api")
#else
            services.AddHq(DatabaseType.Sqlite, _configuration.GetSection("HQ"))
                .AddAuthorizationPolicies(_configuration.GetSection("HQ").GetSection("Security"))
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
