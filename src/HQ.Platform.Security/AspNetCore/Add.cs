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
using System.Diagnostics;
using System.Linq;
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
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
			if(configureSecurityAction != null)
				services.Configure(configureSecurityAction);
			services.ConfigureOptions<ConfigureWebServer>();

            AddDynamicAuthorization(services);
            AddCors(services, logger, options.Cors);
            AddAuthentication(services, logger, options.Tokens, options.Cookies, options.Claims);
            AddSuperUser(services, logger, options.SuperUser);
			AddHttps(services, logger, options);

            return services;
        }

        public static void AddDynamicAuthorization(this IServiceCollection services)
        {
	        services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, DynamicAuthorizeModelProvider>());
	        services.Replace(ServiceDescriptor.Singleton<IAuthorizationPolicyProvider, DynamicAuthorizationPolicyProvider>());
        }

        private static void AddCors(IServiceCollection services, ISafeLogger logger, CorsOptions cors)
        {
	        if (!cors.Enabled)
		        return;

	        logger?.Trace(() => "CORS enabled.");

	        services.AddCors(o =>
	        {
		        o.AddPolicy(Constants.Security.Policies.CorsPolicy, builder =>
		        {
			        builder
				        .WithOrigins(cors.Origins ?? new[] {"*"})
				        .WithMethods(cors.Methods ?? new[] {"*"})
				        .WithHeaders(cors.Headers ?? new[] {"*"})
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

        private static void AddHttps(IServiceCollection services, ISafeLogger logger, SecurityOptions options)
        {
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
        }

        private static void AddAuthentication(IServiceCollection services, ISafeLogger logger, TokenOptions tokens, CookieOptions cookies, ClaimOptions claims)
        {
	        if (tokens.Enabled || cookies.Enabled)
	        {
		        logger?.Trace(() => "Authentication enabled.");

		        services.AddAuthentication(tokens, cookies, claims);
	        }

	        if (tokens.Enabled)
	        {
		        services.AddAuthorization(x =>
		        {
			        TryAddDefaultPolicy(services, logger, x, JwtBearerDefaults.AuthenticationScheme);
		        });
	        }

	        if (cookies.Enabled)
	        {
		        services.AddAuthorization(x =>
		        {
			        TryAddDefaultPolicy(services, logger, x, CookieAuthenticationDefaults.AuthenticationScheme);
		        });
	        }
        }

        private static void AddSuperUser(IServiceCollection services, ISafeLogger logger, SuperUserOptions options)
        {
	        if (options.Enabled)
	        {
		        logger?.Trace(() => $"SuperUser enabled.");

		        services.AddAuthorization(x =>
		        {
					if (x.GetPolicy(Constants.Security.Policies.SuperUserOnly) == null)
				        x.AddPolicy(Constants.Security.Policies.SuperUserOnly,
					        builder => { builder.RequireRoleExtended(services, ClaimValues.SuperUser); });
		        });
	        }
        }

        private static void TryAddDefaultPolicy(IServiceCollection services, ISafeLogger logger, AuthorizationOptions x, string scheme)
        {
	        if (x.DefaultPolicy?.AuthenticationSchemes.Count != 0)
	        {
				Trace.WriteLine($"Skipping default policy build; '{string.Join(",", x.DefaultPolicy?.AuthenticationSchemes ?? Enumerable.Empty<string>())}' already registered.");
		        return;
	        }

			Trace.WriteLine($"Registering default policy with scheme '{scheme}'.");
	        x.DefaultPolicy = new AuthorizationPolicyBuilder(scheme)
		        .RequireAuthenticatedUserExtended(services)
		        .Build();
        }
    }
}
