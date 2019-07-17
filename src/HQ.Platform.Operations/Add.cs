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
using System.Collections.Generic;
using System.Reflection;
using HQ.Common;
using HQ.Common.AspNetCore.Models;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.Mvc;
using HQ.Extensions.Metrics;
using HQ.Extensions.Metrics.Reporters.ServerTiming;
using HQ.Extensions.Options;
using HQ.Platform.Operations.Controllers;
using HQ.Platform.Operations.Models;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore;
using HQ.Platform.Security.AspNetCore.Extensions;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Constants = HQ.Common.Constants;

namespace HQ.Platform.Operations
{
    public static class Add
    {
        public static IServiceCollection AddOperationsApi(this IServiceCollection services,
            IHostingEnvironment environment, IConfiguration config)
        {
	        return AddOperationsApi(services, environment, config.Bind);
        }

        public static IServiceCollection AddOperationsApi(this IServiceCollection services,
	        IHostingEnvironment environment, Action<OperationsApiOptions> configureAction = null)
        {
	        Bootstrap.EnsureInitialized();

	        if (!environment.IsDevelopment())
	        {
		        services.AddTransient<IStartupFilter, HealthCheckStartupFilter>();
	        }

	        services.AddValidOptions();
	        services.AddSaveOptions();
	        services.AddScoped<IMetaProvider, OperationsMetaProvider>();

			if(configureAction != null)
				services.Configure(configureAction);

	        services.AddMetrics(c =>
	        {
		        c.AddCheck<OperationsHealthChecks.ServicesHealth>(nameof(OperationsHealthChecks.ServicesHealth),
			        HealthStatus.Unhealthy, new[] { "ops", "startup" });

		        c.AddCheck<OperationsHealthChecks.OptionsHealth>(nameof(OperationsHealthChecks.OptionsHealth),
			        HealthStatus.Unhealthy, new[] { "ops", "startup" });

		        c.AddServerTimingReporter(o =>
		        {
			        o.Enabled = true;
			        o.Filter = "*";
			        o.Rendering = ServerTimingRendering.Verbose;
			        o.AllowedOrigins = "*";
		        });
	        });

	        return services;
        }

        public static IServiceCollection AddConfigurationApi(this IServiceCollection services, IConfiguration securityConfig, string rootPath = "/ops")
        {
	        return AddConfigurationApi(services, securityConfig.Bind, rootPath);
        }

        public static IServiceCollection AddConfigurationApi(this IServiceCollection services, Action<SecurityOptions> configureSecurity = null, string rootPath = "/ops")
        {
	        services.AddMvc()
		        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
		        .AddConfigurationApi(configureSecurity, rootPath);

	        return services;
        }

        internal static IMvcBuilder AddConfigurationApi(this IMvcBuilder mvcBuilder, IConfiguration securityConfig, string rootPath = "/ops")
        {
            return AddConfigurationApi(mvcBuilder, securityConfig.Bind, rootPath);
        }

		internal static IMvcBuilder AddConfigurationApi(this IMvcBuilder mvcBuilder, Action<SecurityOptions> configureSecurity = null, string rootPath = "/ops")
        {
            if (configureSecurity != null)
	            mvcBuilder.Services.Configure(configureSecurity);

            mvcBuilder.Services.AddValidOptions();
            mvcBuilder.Services.AddSaveOptions();
			mvcBuilder.Services.AddDynamicAuthorization();
			mvcBuilder.Services.AddAuthorization(x =>
            {
                var securityOptions = new SecurityOptions();
                configureSecurity?.Invoke(securityOptions);

                x.AddPolicy(Constants.Security.Policies.ManageConfiguration, b =>
                {
                    b.RequireAuthenticatedUserExtended(mvcBuilder.Services);
                    b.RequireClaimExtended(
	                    mvcBuilder.Services, 
	                    securityOptions.Claims.PermissionClaim, 
	                    ClaimValues.ManageConfiguration);
                });
            });
			
			mvcBuilder.AddControllerFeature<ConfigurationController>();

			mvcBuilder.Services.AddSingleton<IDynamicComponent>(r =>
            {
                return new ConfigurationComponent
                {
                    RouteTemplate = () =>
                    {
                        if (!string.IsNullOrWhiteSpace(rootPath))
                            return rootPath;
                        var o = r.GetRequiredService<IOptions<OperationsApiOptions>>();
                        return o.Value.RootPath ?? string.Empty;
                    }
                };
            });

            return mvcBuilder;
        }

		public static IServiceCollection AddMetaApi(this IServiceCollection services, IConfiguration securityConfig, string rootPath = "/ops")
		{
			return AddMetaApi(services, securityConfig.Bind, rootPath);
		}

		public static IServiceCollection AddMetaApi(this IServiceCollection services, Action<SecurityOptions> configureSecurity = null, string rootPath = "/ops")
		{
			services.AddMvc()
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
				.AddMetaApi(configureSecurity, rootPath);

			return services;
		}

		private static IMvcBuilder AddMetaApi(this IMvcBuilder mvcBuilder, IConfiguration securityConfig, string rootPath = "/ops")
		{
			return AddMetaApi(mvcBuilder, securityConfig.Bind, rootPath);
		}

		private static IMvcBuilder AddMetaApi(this IMvcBuilder mvcBuilder, Action<SecurityOptions> configureSecurity = null, string rootPath = "/ops")
		{
			if (configureSecurity != null)
				mvcBuilder.Services.Configure(configureSecurity);

			mvcBuilder.Services.AddValidOptions();
			mvcBuilder.Services.AddSaveOptions();
			mvcBuilder.Services.AddDynamicAuthorization();

			mvcBuilder.AddControllerFeature<MetaController>();

			mvcBuilder.Services.AddAuthorization(x =>
			{
				var securityOptions = new SecurityOptions();
				configureSecurity?.Invoke(securityOptions);

				x.AddPolicy(Constants.Security.Policies.ManageConfiguration, b =>
				{
					b.RequireAuthenticatedUserExtended(mvcBuilder.Services);
					b.RequireClaimExtended(mvcBuilder.Services, securityOptions.Claims.PermissionClaim, ClaimValues.ManageConfiguration);
				});
			});

			mvcBuilder.ConfigureApplicationPartManager(x =>
			{
				var typeInfo = new List<TypeInfo> { typeof(MetaController).GetTypeInfo() };

				x.ApplicationParts.Add(new DynamicControllerApplicationPart(typeInfo));
			});

			mvcBuilder.Services.AddSingleton<IDynamicComponent>(r =>
			{
				return new MetaComponent
				{
					RouteTemplate = () =>
					{
						if (!string.IsNullOrWhiteSpace(rootPath))
							return rootPath;
						var o = r.GetRequiredService<IOptions<OperationsApiOptions>>();
						return o.Value.RootPath ?? string.Empty;
					}
				};
			});

			mvcBuilder.Services.AddSwaggerGen(c =>
			{
				c.EnableAnnotations();
				c.SwaggerDoc("swagger", new Info { Title = "Sample API", Version = "v1" });
				c.DescribeAllEnumsAsStrings();
			});

			mvcBuilder.Services.TryAddSingleton<IMetaVersionProvider, NoMetaVersionProvider>();
			mvcBuilder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<IMetaProvider, ApiExplorerMetaProvider>());
			
			return mvcBuilder;
		}
	}
}
