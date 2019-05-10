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
using System.IO.Compression;
using HQ.Common;
using HQ.Extensions.Caching;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Api.Extensions;
using HQ.Platform.Api.Filters;
using HQ.Platform.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HQ.Platform.Api
{
    public static class Add
    {
        public static IServiceCollection AddPublicApi(this IServiceCollection services, IConfiguration config)
        {
            Bootstrap.EnsureInitialized();

            services.Configure<PublicApiOptions>(config);

            services.AddCors();
            services.AddHttpCaching();
            services.AddGzipCompression();

            services.AddSingleton<IEnumerable<ITextTransform>>(r => new ITextTransform[] {new CamelCase(), new SnakeCase(), new PascalCase()});
            services.AddSingleton<IConfigureOptions<MvcOptions>, PublicApiMvcConfiguration>();
            services.AddSingleton(r => JsonConvert.DefaultSettings());

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
            services.AddScoped(r => new HttpCacheFilterAttribute(r.GetRequiredService<IETagGenerator>(),
                r.GetRequiredService<IHttpCache>(), r.GetRequiredService<JsonSerializerSettings>()));
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

        #region Multi-Tenancy

        public static IServiceCollection AddMultiTenancy<TTenant>(this IServiceCollection services,
            IConfiguration config)
            where TTenant : class, new()
        {
            return services.AddMultiTenancy<TTenant>(config.Bind);
        }

        public static IServiceCollection AddMultiTenancy<TTenant>(this IServiceCollection services,
            Action<MultiTenancyOptions> configureAction = null)
            where TTenant : class, new()
        {
            return services.AddMultiTenancy<DefaultTenantContextResolver<TTenant>, TTenant>(configureAction);
        }

        public static IServiceCollection AddMultiTenancy<TTenantResolver, TTenant>(this IServiceCollection services,
            IConfiguration config)
            where TTenantResolver : class, ITenantContextResolver<TTenant>
            where TTenant : class, new()
        {
            return services.AddMultiTenancy<TTenantResolver, TTenant>(config.Bind);
        }

        public static IServiceCollection AddMultiTenancy<TTenantResolver, TTenant>(this IServiceCollection services,
            Action<MultiTenancyOptions> configureAction = null)
            where TTenantResolver : class, ITenantContextResolver<TTenant>
            where TTenant : class, new()
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            if (configureAction != null)
            {
                services.Configure(configureAction);
            }

            services.AddInProcessCache();
            services.AddScoped<ITenantContextResolver<TTenant>, TTenantResolver>();
            services.AddScoped(r => r.GetService<IHttpContextAccessor>()?.HttpContext?.GetTenantContext<TTenant>());
            services.AddScoped<ITenantContext<TTenant>>(r => new TenantContextWrapper<TTenant>(r.GetService<TTenant>()));

            return services;
        }

        #endregion
    }
}
