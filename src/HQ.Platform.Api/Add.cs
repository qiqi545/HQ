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
using ActiveRoutes;
using HQ.Common;
using HQ.Common.Models;
using HQ.Common.Serialization;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Runtime;
using HQ.Data.Contracts.Schema.Configuration;
using HQ.Data.Contracts.Versioning;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Implementation;
using HQ.Extensions.Caching;
using HQ.Extensions.Caching.AspNetCore.Mvc;
using HQ.Extensions.DependencyInjection.AspNetCore;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Api.Extensions;
using HQ.Platform.Api.Filters;
using HQ.Platform.Api.Models;
using HQ.Platform.Api.Runtime;
using HQ.Platform.Api.Schemas;
using HQ.Platform.Security;
using HQ.Platform.Security.AspNetCore.Extensions;
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
using Constants = HQ.Common.Constants;
using ActiveOptions;

namespace HQ.Platform.Api
{
	public static class Add
	{
		public static IServiceCollection AddPlatformApi(this IServiceCollection services, IConfiguration config)
		{
			return services.AddPlatformApi(config.FastBind);
		}

		public static IServiceCollection AddPlatformApi(this IServiceCollection services,
			Action<ApiOptions> configureAction = null)
		{
			Bootstrap.EnsureInitialized();

			services.Configure(configureAction);

			services.AddForwardedHeaders();
			services.AddHttpCaching();
			services.AddCanonicalRoutes();
			services.AddGzipCompression();
			services.AddSingleton<IEnumerable<ITextTransform>>(r =>
				new ITextTransform[] {new CamelCase(), new SnakeCase(), new PascalCase()});
			services.AddOptions<RouteOptions>().Configure<IOptions<ApiOptions>>((o, x) =>
			{
				o.AppendTrailingSlash = x.Value.CanonicalRoutes.AppendTrailingSlash;
				o.LowercaseUrls = x.Value.CanonicalRoutes.LowercaseUrls;
				o.LowercaseQueryStrings = x.Value.CanonicalRoutes.LowercaseQueryStrings;
			});
			services.AddSingleton<IConfigureOptions<MvcOptions>, PlatformApiMvcConfiguration>();
			services.AddSingleton(r => JsonConvert.DefaultSettings());

			services.Replace(ServiceDescriptor.Singleton<IMetaVersionProvider, PlatformMetaVersionProvider>());
			services.AddScoped<IMetaParameterProvider>(r =>
				r.GetRequiredService<IOptionsMonitor<ApiOptions>>().CurrentValue.JsonConversion);
			return services;
		}

		internal static IServiceCollection AddForwardedHeaders(this IServiceCollection services)
		{
			services.Configure<ForwardedHeadersOptions>(options =>
			{
				// Needed for proxy, cohort analysis, domain-based multi-tenancy, etc.
				options.ForwardedHeaders =
					ForwardedHeaders.XForwardedFor | // context.Connection.RemoteIpAddress
					ForwardedHeaders.XForwardedProto | // context.Request.Scheme
					ForwardedHeaders.XForwardedHost; // context.Request.Host
			});
			return services;
		}

		internal static IServiceCollection AddHttpCaching(this IServiceCollection services)
		{
			services.TryAddSingleton<IHttpCache, InProcessHttpCache>();
			services.TryAddSingleton<IETagGenerator, WeakETagGenerator>();
			services.AddScoped(r => new HttpCacheFilterAttribute(r.GetRequiredService<IETagGenerator>(),
				r.GetRequiredService<IHttpCache>(), r.GetRequiredService<JsonSerializerSettings>()));
			return services;
		}

		internal static IServiceCollection AddCanonicalRoutes(this IServiceCollection services)
		{
			services.AddSingleton(r => new CanonicalRoutesResourceFilter(r.GetRequiredService<IOptions<ApiOptions>>()));
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
			return services.AddVersioning(config.FastBind);
		}

		public static IServiceCollection AddVersioning(this IServiceCollection services,
			Action<VersioningOptions> configureAction = null)
		{
			return services.AddVersioning<DefaultVersionContextResolver>(configureAction);
		}

		public static IServiceCollection AddVersioning<TVersionResolver>(this IServiceCollection services,
			IConfiguration config)
			where TVersionResolver : class, IVersionContextResolver
		{
			return services.AddVersioning<TVersionResolver>(config.FastBind);
		}

		public static IServiceCollection AddVersioning<TVersionResolver>(this IServiceCollection services,
			Action<VersioningOptions> configureAction = null) where TVersionResolver : class, IVersionContextResolver
		{
			services.AddHttpContextAccessor();

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
			return services.AddMultiTenancy<TTenant, TApplication>(config.FastBind);
		}

		public static IServiceCollection AddMultiTenancy<TTenant, TApplication>(this IServiceCollection services,
			Action<MultiTenancyOptions> configureAction = null)
			where TTenant : class, new()
			where TApplication : class, new()
		{
			return services
				.AddMultiTenancy<DefaultTenantContextResolver<TTenant>, TTenant,
					DefaultApplicationContextResolver<TApplication>, TApplication>(configureAction);
		}

		public static IServiceCollection AddMultiTenancy<TTenantResolver, TTenant, TApplicationResolver, TApplication>(
			this IServiceCollection services,
			IConfiguration config)
			where TTenantResolver : class, ITenantContextResolver<TTenant>
			where TTenant : class, new()
			where TApplicationResolver : class, IApplicationContextResolver<TApplication>
			where TApplication : class, new()
		{
			return services.AddMultiTenancy<TTenantResolver, TTenant, TApplicationResolver, TApplication>(
				config.FastBind);
		}

		public static IServiceCollection AddMultiTenancy<TTenantResolver, TTenant, TApplicationResolver, TApplication>(
			this IServiceCollection services,
			Action<MultiTenancyOptions> configureAction = null)
			where TTenantResolver : class, ITenantContextResolver<TTenant>
			where TTenant : class, new()
			where TApplicationResolver : class, IApplicationContextResolver<TApplication>
			where TApplication : class, new()
		{
			services.AddHttpContextAccessor();

			if (configureAction != null)
				services.Configure(configureAction);

			services.AddInProcessCache();

			services.AddScoped<ITenantContextResolver<TTenant>, TTenantResolver>();
			services.AddScoped(r => r.GetService<IHttpContextAccessor>()?.HttpContext?.GetTenantContext<TTenant>());
			services.AddScoped<ITenantContext<TTenant>>(r =>
				new TenantContextWrapper<TTenant>(r.GetService<TTenant>()));

			services.AddScoped<IApplicationContextResolver<TApplication>, TApplicationResolver>();
			services.AddScoped(r =>
				r.GetService<IHttpContextAccessor>()?.HttpContext?.GetApplicationContext<TApplication>());
			services.AddScoped<IApplicationContext<TApplication>>(r =>
				new ApplicationContextWrapper<TApplication>(r.GetService<TApplication>()));

			return services;
		}

		#endregion

		#region Schema

		public static IServiceCollection AddSchemaApi(this IServiceCollection services, IConfiguration config)
		{
			return services.AddSchemaApi(config.FastBind);
		}

		public static IServiceCollection AddSchemaApi(this IServiceCollection services,
			Action<SchemaOptions> configureAction = null)
		{
			var mvcBuilder = services.AddMvcCore();
			mvcBuilder.AddSchemaApi(configureAction);
			return mvcBuilder.Services;
		}

		public static IMvcCoreBuilder AddSchemaApi(this IMvcCoreBuilder mvcBuilder, IConfiguration config)
		{
			mvcBuilder.AddSchemaApi(config.FastBind);
			return mvcBuilder;
		}

		public static IMvcCoreBuilder AddSchemaApi(this IMvcCoreBuilder mvcBuilder,
			Action<SchemaOptions> configureAction = null)
		{
			if (configureAction != null)
				mvcBuilder.Services.Configure(configureAction);

			mvcBuilder.Services.AddTypeResolver();

			mvcBuilder.AddActiveRoute<SchemaController, SchemaComponent, SchemaOptions>();
			mvcBuilder.AddDefaultAuthorization(Constants.Security.Policies.ManageSchemas, ClaimValues.ManageSchemas);
			return mvcBuilder;
		}

		#endregion

		#region Runtime

		public static RuntimeBuilder AddRuntimeApi(this IServiceCollection services, IConfiguration config)
		{
			return services.AddRuntimeApi(config.FastBind);
		}

		public static RuntimeBuilder AddRuntimeApi(this IServiceCollection services,
			Action<RuntimeOptions> configureAction = null)
		{
			var mvcBuilder = services.AddMvcCore();
			mvcBuilder.AddRuntimeApi(configureAction);
			return new RuntimeBuilder(mvcBuilder.Services);
		}

		public static RuntimeBuilder AddRuntimeApi(this IMvcCoreBuilder mvcBuilder, IConfiguration config)
		{
			return mvcBuilder.AddRuntimeApi(config.FastBind);
		}

		public static RuntimeBuilder AddRuntimeApi(this IMvcCoreBuilder mvcBuilder,
			Action<RuntimeOptions> configureAction = null)
		{
			if (configureAction != null)
				mvcBuilder.Services.Configure(configureAction);
			
			mvcBuilder.AddActiveRoute<RuntimeController, RuntimeComponent, RuntimeOptions>();
			mvcBuilder.AddDefaultAuthorization(Constants.Security.Policies.ManageObjects, ClaimValues.ManageObjects);
			return new RuntimeBuilder(mvcBuilder.Services);
		}

		public static RuntimeBuilder AddSqlRuntimeStores<TDatabase>
		(
			this RuntimeBuilder builder,
			string connectionString,
			ConnectionScope scope,
			Action<IDbCommand, Type, IServiceProvider> onCommand = null,
			Action<IDbConnection, IServiceProvider> onConnection = null
		)
			where TDatabase : class, IConnectionFactory, new()
		{
			if (scope == ConnectionScope.ByRequest)
				builder.Services.AddHttpContextAccessor();

			builder.Services.AddDatabaseConnection<RuntimeBuilder, TDatabase>(connectionString, scope, new[] { new HttpAccessorExtension() }, onConnection,onCommand);
			builder.Services.AddScoped<IObjectGetRepository<long>, SqlObjectGetRepository>();

			return builder;
		}

		#endregion
	}
}