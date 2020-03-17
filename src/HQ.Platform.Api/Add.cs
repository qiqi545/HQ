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
using ActiveCaching;
using ActiveRoutes;
using HQ.Common;
using HQ.Common.Models;
using HQ.Common.Serialization;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Runtime;
using HQ.Data.Contracts.Schema.Configuration;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Implementation;
using HQ.Extensions.Caching;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Api.Filters;
using HQ.Platform.Api.Models;
using HQ.Platform.Api.Runtime;
using HQ.Platform.Api.Schemas;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
			services.AddSingleton<IEnumerable<ITextTransform>>(r => new ITextTransform[] {new CamelCase(), new SnakeCase(), new PascalCase()});
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
					ForwardedHeaders.XForwardedFor |	// context.Connection.RemoteIpAddress
					ForwardedHeaders.XForwardedProto |	// context.Request.Scheme
					ForwardedHeaders.XForwardedHost;	// context.Request.Host
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

			builder.Services.AddDatabaseConnection<RuntimeBuilder, TDatabase>(connectionString, scope, onConnection,onCommand);
			builder.Services.AddScoped<IObjectGetRepository<long>, SqlObjectGetRepository>();

			return builder;
		}

		#endregion
	}
}