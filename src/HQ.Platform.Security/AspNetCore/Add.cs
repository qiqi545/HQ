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
using System.Linq;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.AspNetCore.Mvc.Security;
using HQ.Extensions.Logging;
using HQ.Extensions.Options;
using HQ.Platform.Security.AspNetCore.Configuration;
using HQ.Platform.Security.AspNetCore.Extensions;
using HQ.Platform.Security.AspNetCore.Models;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using CorsOptions = HQ.Platform.Security.Configuration.CorsOptions;

namespace HQ.Platform.Security.AspNetCore
{
	public static class Add
	{
		public static IServiceCollection AddSecurityPolicies(this IServiceCollection services,
			IConfiguration securityConfig, 
			IConfiguration superUserConfig,
			ISafeLogger logger)
		{
			return AddSecurityPolicies(services, securityConfig.FastBind, superUserConfig.FastBind, logger);
		}

		public static IServiceCollection AddSecurityPolicies(this IServiceCollection services,
			Action<SecurityOptions> configureSecurityAction = null,
			Action<SuperUserOptions> configureSuperUserAction = null,
			ISafeLogger logger = null)
		{
			Bootstrap.EnsureInitialized();
			Bootstrap.ContractResolver.IgnoreTypes.Add(typeof(KestrelConfigurationLoader));

			var security = new SecurityOptions(true);
			configureSecurityAction?.Invoke(security);

			var superUser = new SuperUserOptions();
			configureSuperUserAction?.Invoke(superUser);
			
			AuthenticationExtensions.MaybeSetSecurityKeys(security);

			if (configureSecurityAction != null)
			{
				services.Configure<SecurityOptions>(o =>
				{
					configureSecurityAction.Invoke(o);
					o.Signing = security.Signing;
					o.Encrypting = security.Encrypting;
				});
			}

			if (configureSuperUserAction != null)
				services.Configure<SuperUserOptions>(configureSuperUserAction.Invoke);

			services.ConfigureOptions<ConfigureWebServer>();

			services.AddDynamicAuthorization();
			services.AddCors(logger, security.Cors);
			services.AddAuthentication(logger, security, superUser);
			services.AddSuperUser(logger, superUser);
			services.AddHttps(logger, security);

			return services;
		}

		private static void AddCors(this IServiceCollection services, ISafeLogger logger, CorsOptions cors)
		{
			if (!cors.Enabled)
				return;

			logger?.Trace(() => "CORS enabled.");

			services.AddRouting(o => { });

			services.AddCors(o =>
			{
				o.AddPolicy(Constants.Security.Policies.CorsPolicy, builder =>
				{
					builder
						.WithOrigins(cors.Origins ?? new[] { "*" })
						.WithMethods(cors.Methods ?? new[] { "*" })
						.WithHeaders(cors.Headers ?? new[] { "*" })
						.WithExposedHeaders(cors.ExposedHeaders ?? new string[0]);

					if (cors.AllowCredentials && cors.Origins?.Length > 0 && cors.Origins[0] != "*")
						builder.AllowCredentials();
					else
						builder.DisallowCredentials();

					if (cors.AllowOriginWildcards)
						builder.SetIsOriginAllowedToAllowWildcardSubdomains();

					if (cors.PreflightMaxAgeSeconds.HasValue)
						builder.SetPreflightMaxAge(TimeSpan.FromSeconds(cors.PreflightMaxAgeSeconds.Value));
				});
			});


			services.AddMvcCommon();
		}

		private static void AddHttps(this IServiceCollection services, ISafeLogger logger, SecurityOptions options)
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

		private static void AddAuthentication(this IServiceCollection services, ISafeLogger logger, 
			SecurityOptions security,
			SuperUserOptions superUser)
		{
			var tokens = security.Tokens;
			var cookies = security.Cookies;
			var claims = security.Claims;

			if (tokens.Enabled || cookies.Enabled || superUser.Enabled)
			{
				if (!tokens.Enabled && !cookies.Enabled && superUser.Enabled)
				{
					logger?.Trace(() => "Authentication enabled for super user only.");
				}
				else
				{
					logger?.Trace(() => "Authentication enabled.");
				}

				services.AddAuthentication(security, superUser, tokens, cookies, claims);
			}

			if (tokens.Enabled || superUser.Enabled)
			{
				services.AddAuthorization(x =>
				{
					TryAddDefaultPolicy(services, logger, x, tokens.Scheme);
				});
			}

			if (cookies.Enabled)
			{
				services.AddAuthorization(x =>
				{
					TryAddDefaultPolicy(services, logger, x, cookies.Scheme);
				});
			}
		}

		private static void AddSuperUser(this IServiceCollection services, ISafeLogger logger, SuperUserOptions options)
		{
			if (options.Enabled)
			{
				logger?.Trace(() => $"SuperUser enabled.");
				services.AddDefaultAuthorization(Constants.Security.Policies.SuperUserOnly, ClaimValues.SuperUser);
			}
		}

		private static void TryAddDefaultPolicy(this IServiceCollection services, ISafeLogger logger, AuthorizationOptions x, string scheme)
		{
			if (x.DefaultPolicy?.AuthenticationSchemes.Count != 0)
			{
				logger?.Info(() => $"Skipping default policy build; '{string.Join(",", x.DefaultPolicy?.AuthenticationSchemes ?? Enumerable.Empty<string>())}' already registered.");
				return;
			}

			logger?.Info(() => $"Registering default policy with scheme '{scheme}'.");
			x.DefaultPolicy = new AuthorizationPolicyBuilder(scheme)
				.RequireAuthenticatedUserExtended(services)
				.Build();
		}
	}
}
