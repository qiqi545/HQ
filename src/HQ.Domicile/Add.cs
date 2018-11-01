// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;
using System.IO.Compression;
using HQ.Domicile.Configuration;
using HQ.Domicile.Filters;
using HQ.Domicile.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HQ.Domicile
{
	public static class Add
	{
		public static IServiceCollection AddPublicApi(this IServiceCollection services, IConfiguration config)
		{
			services.Configure<PublicApiOptions>(config);

			services.AddCors();
			services.AddHttpCaching();
			services.AddGzipCompression();

			services.AddSingleton<IEnumerable<ITextTransform>>(r => new ITextTransform[] {new CamelCase(), new SnakeCase(), new PascalCase() });
			services.AddSingleton(r => JsonConvert.DefaultSettings());
			services.AddSingleton<IConfigureOptions<MvcOptions>, PublicApiMvcConfiguration>();

			return services;
		}

		internal static IServiceCollection AddCors(this IServiceCollection services)
		{
			services.AddCors(o => o.AddDefaultPolicy(builder =>
			{
				builder
					.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader()
					.AllowCredentials();
			}));

			return services;
		}

		internal static IServiceCollection AddHttpCaching(this IServiceCollection services)
		{
			services.AddSingleton<IHttpCache>(r => new MemoryHttpCache(r.GetRequiredService<IMemoryCache>()));
			services.AddSingleton<IETagGenerator, WeakETagGenerator>();
			services.AddScoped(r => new HttpCacheFilter(r.GetRequiredService<IETagGenerator>(),
				r.GetRequiredService<IHttpCache>(), r.GetRequiredService<JsonSerializerSettings>()));
			return services;
		}

		internal static IServiceCollection AddGzipCompression(this IServiceCollection services)
		{
			services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
			services.AddResponseCompression(options =>
			{
				options.EnableForHttps = true;
				options.Providers.Add<GzipCompressionProvider>();
			});
			return services;
		}
	}
}