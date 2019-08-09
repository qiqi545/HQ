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
using System.Data;
using System.IO.Compression;
using HQ.Common;
using HQ.Common.AspNetCore.Models;
using HQ.Common.AspNetCore.Mvc;
using HQ.Common.Serialization;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Mvc.Security;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Implementation;
using HQ.Extensions.Caching;
using HQ.Extensions.Caching.AspNetCore.Mvc;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Api.Extensions;
using HQ.Platform.Api.Filters;
using HQ.Platform.Api.Models;
using HQ.Platform.Api.Runtime;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore.Extensions;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HQ.Platform.Api
{
	public static class Add
    {
        public static IServiceCollection AddPlatformApi(this IServiceCollection services, IConfiguration config)
        {
            return services.AddPlatformApi(config.Bind);
        }

        public static IServiceCollection AddPlatformApi(this IServiceCollection services, Action<PlatformApiOptions> configureAction = null)
        {
            Bootstrap.EnsureInitialized();

            services.Configure(configureAction);

            services.AddForwardedHeaders();
            services.AddHttpCaching();
            services.AddCanonicalRoutes();
            services.AddGzipCompression();
            services.AddSingleton<IEnumerable<ITextTransform>>(r => new ITextTransform[] {new CamelCase(), new SnakeCase(), new PascalCase()});
            services.AddOptions<RouteOptions>().Configure<IOptions<PlatformApiOptions>>((o, x) =>
            {
                o.AppendTrailingSlash = x.Value.CanonicalRoutes.AppendTrailingSlash;
                o.LowercaseUrls = x.Value.CanonicalRoutes.LowercaseUrls;
                o.LowercaseQueryStrings = x.Value.CanonicalRoutes.LowercaseQueryStrings;
            });
            services.AddSingleton<IConfigureOptions<MvcOptions>, PlatformApiMvcConfiguration>();
            services.AddSingleton(r => JsonConvert.DefaultSettings());

            services.Replace(ServiceDescriptor.Singleton<IMetaVersionProvider, PlatformMetaVersionProvider>());

			return services;
        }

        internal static IServiceCollection AddForwardedHeaders(this IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                // Needed for proxy, cohort analysis, domain-based multi-tenancy, etc.
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |        // context.Connection.RemoteIpAddress
                    ForwardedHeaders.XForwardedProto |      // context.Request.Scheme
                    ForwardedHeaders.XForwardedHost;        // context.Request.Host
            });
            return services;
        }

        internal static IServiceCollection AddHttpCaching(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpCache, InProcessHttpCache>();
            services.TryAddSingleton<IETagGenerator, WeakETagGenerator>();
            services.AddScoped(r => new HttpCacheFilterAttribute(r.GetRequiredService<IETagGenerator>(), r.GetRequiredService<IHttpCache>(), r.GetRequiredService<JsonSerializerSettings>()));
            return services;
        }

        internal static IServiceCollection AddCanonicalRoutes(this IServiceCollection services)
        {
            services.AddSingleton(r => new CanonicalRoutesResourceFilter(r.GetRequiredService<IOptions<PlatformApiOptions>>()));
            return services;
        }

        internal static IServiceCollection AddGzipCompression(this IServiceCollection services)
        {
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = false;
                options.Providers.Add<GzipCompressionProvider>();
            });
            return services;
        }
		
        #region Versioning

        public static IServiceCollection AddVersioning(this IServiceCollection services, IConfiguration config)
        {
            return services.AddVersioning(config.Bind);
        }

        public static IServiceCollection AddVersioning(this IServiceCollection services, Action<VersioningOptions> configureAction = null)
        {
            return services.AddVersioning<DefaultVersionContextResolver>(configureAction);
        }

        public static IServiceCollection AddVersioning<TVersionResolver>(this IServiceCollection services, IConfiguration config)
            where TVersionResolver : class, IVersionContextResolver
        {
            return services.AddVersioning<TVersionResolver>(config.Bind);
        }

        public static IServiceCollection AddVersioning<TVersionResolver>(this IServiceCollection services, Action<VersioningOptions> configureAction = null) where TVersionResolver : class, IVersionContextResolver
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            if (configureAction != null)
				services.Configure(configureAction);

			services.AddInProcessCache();
            services.TryAddSingleton<IVersionContextStore, NoVersionContextStore>();
            services.AddScoped<IVersionContextResolver, TVersionResolver>();
            services.AddScoped(r => r.GetService<IHttpContextAccessor>()?.HttpContext?.GetVersionContext());
            return services;
        }

        #endregion

        #region Multi-Tenancy

        public static IServiceCollection AddMultiTenancy<TTenant, TApplication>(this IServiceCollection services,
            IConfiguration config)
            where TTenant : class, new()
            where TApplication : class, new()
        {
            return services.AddMultiTenancy<TTenant, TApplication>(config.Bind);
        }

        public static IServiceCollection AddMultiTenancy<TTenant, TApplication>(this IServiceCollection services,
            Action<MultiTenancyOptions> configureAction = null)
            where TTenant : class, new()
            where TApplication : class, new()
        {
            return services.AddMultiTenancy<DefaultTenantContextResolver<TTenant>, TTenant, DefaultApplicationContextResolver<TApplication>, TApplication>(configureAction);
        }

        public static IServiceCollection AddMultiTenancy<TTenantResolver, TTenant, TApplicationResolver, TApplication>(this IServiceCollection services,
            IConfiguration config)
            where TTenantResolver : class, ITenantContextResolver<TTenant>
            where TTenant : class, new()
            where TApplicationResolver : class, IApplicationContextResolver<TApplication>
            where TApplication : class, new()
        {
            return services.AddMultiTenancy<TTenantResolver, TTenant, TApplicationResolver, TApplication>(config.Bind);
        }

        public static IServiceCollection AddMultiTenancy<TTenantResolver, TTenant, TApplicationResolver, TApplication>(this IServiceCollection services,
            Action<MultiTenancyOptions> configureAction = null)
            where TTenantResolver : class, ITenantContextResolver<TTenant>
            where TTenant : class, new()
            where TApplicationResolver : class, IApplicationContextResolver<TApplication>
            where TApplication : class, new()
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            if (configureAction != null)
				services.Configure(configureAction);

			services.AddInProcessCache();

            services.AddScoped<ITenantContextResolver<TTenant>, TTenantResolver>();
            services.AddScoped(r => r.GetService<IHttpContextAccessor>()?.HttpContext?.GetTenantContext<TTenant>());
            services.AddScoped<ITenantContext<TTenant>>(r => new TenantContextWrapper<TTenant>(r.GetService<TTenant>()));

            services.AddScoped<IApplicationContextResolver<TApplication>, TApplicationResolver>();
            services.AddScoped(r => r.GetService<IHttpContextAccessor>()?.HttpContext?.GetApplicationContext<TApplication>());
            services.AddScoped<IApplicationContext<TApplication>>(r => new ApplicationContextWrapper<TApplication>(r.GetService<TApplication>()));

            return services;
        }

		#endregion

		#region Runtime

		public static IServiceCollection AddSchemaDiscovery(this IServiceCollection services)
		{
			services.AddTypeDiscovery();
			
			return services;
		}


		public static IServiceCollection AddRuntimeApi(this IServiceCollection services, IConfiguration config)
		{
			return services.AddRuntimeApi(config.Bind);
		}

		public static IServiceCollection AddRuntimeApi(this IServiceCollection services, Action<RuntimeOptions> configureAction = null)
		{
			var mvcBuilder = services.AddMvc();
			mvcBuilder.AddRuntimeApi();
			return services;
		}

		public static IMvcBuilder AddRuntimeApi(this IMvcBuilder mvcBuilder, IConfiguration config)
		{
			return mvcBuilder.AddRuntimeApi(config.Bind);
		}

		public static IMvcBuilder AddRuntimeApi(this IMvcBuilder mvcBuilder, Action<RuntimeOptions> configureAction = null)
		{
			if (configureAction != null)
				mvcBuilder.Services.Configure(configureAction);

			mvcBuilder.AddControllerFeature<RuntimeController>();
			mvcBuilder.AddComponentFeature<RuntimeComponent, RuntimeOptions>();

			mvcBuilder.Services.AddDynamicAuthorization();
			mvcBuilder.Services.AddAuthorization(x =>
			{
				var serviceProvider = mvcBuilder.Services.BuildServiceProvider();
				var options = serviceProvider.GetRequiredService<IOptions<SecurityOptions>>();

				x.AddPolicy(Constants.Security.Policies.ManageObjects, b =>
				{
					b.RequireAuthenticatedUserExtended(mvcBuilder.Services);
					b.RequireClaimExtended(mvcBuilder.Services, options.Value.Claims.PermissionClaim, ClaimValues.ManageObjects);
				});
			});

			return mvcBuilder;
		}

		public static IServiceCollection AddSqlRuntimeStores<TDatabase>
		(
			this IServiceCollection services,
			string connectionString,
			ConnectionScope scope,
			Action<IDbCommand, Type, IServiceProvider> onCommand = null,
			Action<IDbConnection, IServiceProvider> onConnection = null
		)
			where TDatabase : class, IConnectionFactory, new()
		{
			if (scope == ConnectionScope.ByRequest)
				services.AddHttpContextAccessor();

			services.AddDatabaseConnection<IObjectGetRepository, TDatabase>(connectionString, scope, onConnection, onCommand);
			services.AddScoped<IObjectGetRepository, SqlObjectGetRepository>();

			return services;
		}

		#endregion
	}
}
