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

using System;
using HQ.Common;
using HQ.Extensions.Logging;
using HQ.Platform.Security.AspNetCore.Configuration;
using HQ.Platform.Security.AspNetCore.Extensions;
using HQ.Platform.Security.AspNetCore.Models;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Security.AspNetCore
{
    public static class Add
    {
        public static IServiceCollection AddSecurityPolicies(this IServiceCollection services, IConfiguration config, ISafeLogger logger)
        {
            return AddSecurityPolicies(services, config.Bind, logger);
        }

        public static IServiceCollection AddSecurityPolicies(this IServiceCollection services,
	        Action<SecurityOptions> configureSecurityAction = null, ISafeLogger logger = null)
        {
            Bootstrap.EnsureInitialized();
            Bootstrap.ContractResolver.IgnoreTypes.Add(typeof(KestrelConfigurationLoader));

            var options = new SecurityOptions(true);
            configureSecurityAction?.Invoke(options);
            services.Configure<SecurityOptions>(o => { configureSecurityAction?.Invoke(o); });
            services.ConfigureOptions<ConfigureWebServer>();

            var cors = options.Cors;
            if (cors.Enabled)
            {
                logger?.Trace(()=> "CORS enabled.");

                services.AddCors(o =>
                {
                    o.AddPolicy(Constants.Security.Policies.CorsPolicy, builder =>
                    {
                        builder
                            .WithOrigins(cors.Origins ?? new[] { "*" })
                            .WithMethods(cors.Methods ?? new[] { "*" })
                            .WithHeaders(cors.Headers ?? new []{ "*" })
                            .WithExposedHeaders(cors.ExposedHeaders ?? new string[0]);
                        
                        if (cors.AllowCredentials)
                            builder.AllowCredentials();
                        else
                            builder.DisallowCredentials();

                        if (cors.AllowOriginWildcards)
                            builder.SetIsOriginAllowedToAllowWildcardSubdomains();

                        if (cors.PreflightMaxAgeSeconds.HasValue)
                            builder.SetPreflightMaxAge(TimeSpan.FromSeconds(cors.PreflightMaxAgeSeconds.Value));
                    });
                });

                services.AddMvc(o =>
                {
	                o.Filters.Add(new CorsAuthorizationFilterFactory(Constants.Security.Policies.CorsPolicy));
                });
            }
            
            if (options.Tokens.Enabled || options.Cookies.Enabled)
            {
                logger?.Trace(() => "Authentication enabled.");

                services.AddAuthentication(options);
            }

            if (options.Tokens.Enabled)
            {
				services.AddAuthorization(x =>
				{
					AddDefaultPolicy(services, logger, x, options, JwtBearerDefaults.AuthenticationScheme);
				});
			}

			if (options.Cookies.Enabled)
			{
				services.AddAuthorization(x =>
				{
					AddDefaultPolicy(services, logger, x, options, CookieAuthenticationDefaults.AuthenticationScheme);
				});
			}

            if (options.Https.Enabled)
            {
                logger?.Trace(() => "HTTPS enabled.");

                services.AddHttpsRedirection(o =>
                {
                    o.HttpsPort = null;
                    o.RedirectStatusCode = options.Https.Hsts.Enabled ? 307 : 301;
                });

                if (options.Https.Hsts.Enabled)
                {
                    logger?.Trace(() => "HSTS enabled.");

                    services.AddHsts(o =>
                    {
                        o.MaxAge = options.Https.Hsts.HstsMaxAge;
                        o.IncludeSubDomains = options.Https.Hsts.IncludeSubdomains;
                        o.Preload = options.Https.Hsts.Preload;
                    });
                }
            }

            return services;
        }

        private static void AddDefaultPolicy(IServiceCollection services, ISafeLogger logger, AuthorizationOptions x,
	        SecurityOptions options, string scheme)
        {
	        x.DefaultPolicy = new AuthorizationPolicyBuilder(scheme)
		        .RequireAuthenticatedUserExtended(services, options)
		        .Build();

	        if (options.SuperUser.Enabled)
	        {
		        logger?.Trace(() => $"SuperUser enabled for {scheme}");

		        x.AddPolicy(Constants.Security.Policies.SuperUserOnly,
			        builder => { builder.RequireRoleExtended(services, options, ClaimValues.SuperUser); });
	        }
        }
    }
}
