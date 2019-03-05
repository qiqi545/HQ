using HQ.Data.Sql.Sqlite;
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
#if (DocumentDb)
            services.AddHq(DatabaseType.DocumentDb, _configuration.GetSection("HQ")).AddGenerated<DocumentDbBatchOptions>(_configuration.GetSection("HQ").GetSection("Security"), "/api")
#elif (SqlServer)
            services.AddHq(DatabaseType.SqlServer, _configuration.GetSection("HQ")).AddGenerated<SqlServerOptions>(_configuration.GetSection("HQ").GetSection("Security"), "/api")
#elif (MySql)
            services.AddHq(DatabaseType.MySql, _configuration.GetSection("HQ")).AddGenerated<MySqlOptions>(_configuration.GetSection("HQ").GetSection("Security"), "/api")
#else
            services.AddHq(DatabaseType.Sqlite, _configuration.GetSection("HQ")).AddGenerated<SqliteOptions>(_configuration.GetSection("HQ").GetSection("Security"), "/api")
#endif
                .AddUi();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHq();
        }
    }
}
