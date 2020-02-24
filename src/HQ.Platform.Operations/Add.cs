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
using ActiveOptions;
using ActiveRoutes;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Common.Models;
using HQ.Data.Contracts.AspNetCore.Mvc;
using HQ.Data.Contracts.Schema.Models;
using HQ.Extensions.Metrics;
using HQ.Extensions.Metrics.Reporters.ServerTiming;
using HQ.Platform.Operations.Configuration;
using HQ.Platform.Operations.Controllers;
using HQ.Platform.Operations.Models;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Constants = HQ.Common.Constants;

namespace HQ.Platform.Operations
{
	public static class Add
	{
		public static IServiceCollection AddOperationsApi(this IServiceCollection services, IConfiguration config)
		{
			return AddOperationsApi(services, config.FastBind);
		}

		public static IServiceCollection AddOperationsApi(this IServiceCollection services, Action<OperationsApiOptions> configureAction = null)
		{
			Bootstrap.EnsureInitialized();

			if (configureAction != null)
				services.Configure(configureAction);

			var options = new OperationsApiOptions();
			configureAction?.Invoke(options);

			if(options.EnableHealthChecks)
				services.AddTransient<IStartupFilter, HealthCheckStartupFilter>();

			services.AddValidOptions();
			services.AddSaveOptions();

			services.AddScoped<IMetaProvider, OperationsMetaProvider>();

			services.AddMetrics(c =>
			{
				c.AddCheck<OperationsHealthChecks.ServicesHealth>(nameof(OperationsHealthChecks.ServicesHealth),
					HealthStatus.Unhealthy, new[] {"ops", "startup"});

				c.AddCheck<OperationsHealthChecks.OptionsHealth>(nameof(OperationsHealthChecks.OptionsHealth),
					HealthStatus.Unhealthy, new[] {"ops", "startup"});

				c.AddServerTimingReporter(o =>
				{
					o.Enabled = true;
					o.Filter = "*";
					o.Rendering = ServerTimingRendering.Verbose;
					o.AllowedOrigins = "*";
				});
			});

			services.AddDefaultAuthorization(Constants.Security.Policies.AccessOperations, ClaimValues.AccessOperations);
			return services;
		}

		public static IServiceCollection AddConfigurationApi(this IServiceCollection services, IConfigurationRoot configurationRoot, IConfiguration config)
		{
			return AddConfigurationApi(services, configurationRoot, config.FastBind);
		}

		public static IServiceCollection AddConfigurationApi(this IServiceCollection services, IConfigurationRoot configurationRoot, Action<ConfigurationApiOptions> configureAction = null)
		{
			services.AddSingleton(configurationRoot);
			services.AddActiveRouting(mvcBuilder =>
			{
				mvcBuilder.AddConfigurationApi(configureAction);
			});
			return services;
		}

		public static IMvcCoreBuilder AddConfigurationApi(this IMvcCoreBuilder mvcBuilder, IConfigurationRoot configurationRoot, IConfiguration config)
		{
			mvcBuilder.Services.AddSingleton(configurationRoot);
			return AddConfigurationApi(mvcBuilder, config.FastBind);
		}

		public static IMvcCoreBuilder AddConfigurationApi(this IMvcCoreBuilder mvcBuilder, Action<ConfigurationApiOptions> configureAction = null)
		{
			if (configureAction != null)
				mvcBuilder.Services.Configure(configureAction);

			mvcBuilder.Services.AddValidOptions();
			mvcBuilder.Services.AddSaveOptions();

			mvcBuilder.Services.AddSingleton<ConfigurationService>();
			mvcBuilder.AddActiveRoute<ConfigurationController, ConfigurationComponent, ConfigurationApiOptions>();
			mvcBuilder.AddDefaultAuthorization(Constants.Security.Policies.ManageConfiguration, ClaimValues.ManageConfiguration);

			return mvcBuilder;
		}

		public static IServiceCollection AddMetaApi(this IServiceCollection services, IConfiguration config)
		{
			return AddMetaApi(services, config.FastBind);
		}

		public static IServiceCollection AddMetaApi(this IServiceCollection services, Action<MetaApiOptions> configureAction = null)
		{
			services.AddMvcCore().AddMetaApi(configureAction);
			return services;
		}

		public static IMvcCoreBuilder AddMetaApi(this IMvcCoreBuilder builder, IConfiguration config)
		{
			return AddMetaApi(builder, config.FastBind);
		}

		public static IMvcCoreBuilder AddMetaApi(this IMvcCoreBuilder mvcBuilder, Action<MetaApiOptions> configureAction = null)
		{
			if (configureAction != null)
				mvcBuilder.Services.Configure(configureAction);

			mvcBuilder.Services.TryAddSingleton<IApplicationVersionStore, NoApplicationVersionStore>();
			mvcBuilder.Services.TryAddSingleton<ISchemaVersionStore, NoSchemaVersionStore>();

			mvcBuilder.Services.AddValidOptions();
			mvcBuilder.Services.AddSaveOptions();

			mvcBuilder.AddActiveRoute<MetaController, MetaComponent, MetaApiOptions>();
			mvcBuilder.AddDefaultAuthorization(Constants.Security.Policies.AccessMeta, ClaimValues.AccessMeta);
			
			mvcBuilder.Services.TryAddSingleton<IMetaVersionProvider, NoMetaVersionProvider>();
			mvcBuilder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<IMetaProvider, ApiExplorerMetaProvider>());

			return mvcBuilder;
		}

		public static IServiceCollection AddGraphViz(this IServiceCollection services)
		{
			services.AddMvcCommon(o =>
			{
				o.OutputFormatters.Add(new GraphVizOutputFormatter());
			});

			return services;
		}
	}
}