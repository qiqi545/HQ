using HQ.Tokens.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Tokens
{
    public static class Add
    {
        public static IServiceCollection AddTokens(this IServiceCollection services, IConfiguration config)
        {
            // for now
            var options = new SecurityOptions();
            config.Bind(options);

            // for later
            services.Configure<SecurityOptions>(config);

            if (options.Tokens.Enabled)
            {
                services.AddAuthentication(options);
            }

            services.AddAuthorization(x =>
            {
                var builder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
                builder.RequireAuthenticatedUser();
                x.DefaultPolicy = builder.Build();
            });
            
            return services;
        }
    }
}
