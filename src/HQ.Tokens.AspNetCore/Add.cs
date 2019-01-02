using HQ.Common;
using HQ.Tokens.AspNetCore.Extensions;
using HQ.Tokens.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Tokens.AspNetCore
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
                services.AddAuthentication(options);

            services.AddAuthorization(x =>
            {
                x.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUserExtended(services, options)
                    .Build();

                if (options.SuperUser.Enabled)
                    x.AddPolicy(Constants.Security.Policies.SuperUserOnly,
                        builder => { builder.RequireRoleExtended(services, options, ClaimValues.SuperUser); });
            });

            return services;
        }
    }
}
