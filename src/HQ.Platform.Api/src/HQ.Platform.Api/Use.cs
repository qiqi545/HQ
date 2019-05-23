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
using System.Net;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Api.Extensions;
using HQ.Platform.Api.Filters;
using HQ.Platform.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Api
{
    public static class Use
    {
        public static IApplicationBuilder UsePlatformApi(this IApplicationBuilder app, bool snapshot = true)
        {
            app.UseCors();
            app.UseResponseCompression();

            app.UseCanonicalRoutes(snapshot);
            app.UseMethodOverrides(snapshot);
            app.UseResourceRewriting(snapshot);
            app.UseRequestLimiting(snapshot);
            app.UseJsonMultiCase(snapshot);

            return app;
        }

        private static IApplicationBuilder UseCanonicalRoutes(this IApplicationBuilder app, bool snapshot)
        {
            if (snapshot)
            {
                return app.FeatureEnabled<CanonicalRoutesOptions, PublicApiOptions>(out var options)
                    ? app.Use(async (context, next) => { await ExecuteFeature(context, options, next); })
                    : app;
            }

            return app.Use(async (context, next) =>
            {
                if (context.FeatureEnabled<CanonicalRoutesOptions, PublicApiOptions>(out var options))
                {
                    await ExecuteFeature(context, options, next);
                }
            });

            async Task ExecuteFeature(HttpContext c, CanonicalRoutesOptions o, Func<Task> next)
            {
                if (string.Equals(c.Request.Method, Constants.HttpVerbs.Get, StringComparison.OrdinalIgnoreCase))
                {
                    if (!CanonicalRoutesResourceFilter.TryGetCanonicalRoute(c.Request, o, out var redirectToUrl))
                    {
                        c.Response.Redirect(redirectToUrl, true);
                        return;
                    }
                }

                await next();
            }
        }

        private static IApplicationBuilder UseMethodOverrides(this IApplicationBuilder app, bool snapshot)
        {
            if (snapshot)
            {
                return app.FeatureEnabled<MethodOverrideOptions, PublicApiOptions>(out var options)
                    ? app.Use(async (context, next) => { await ExecuteFeature(context, options, next); })
                    : app;
            }

            return app.Use(async (context, next) =>
            {
                if (context.FeatureEnabled<MethodOverrideOptions, PublicApiOptions>(out var options))
                {
                    await ExecuteFeature(context, options, next);
                }
            });

            async Task ExecuteFeature(HttpContext c, MethodOverrideOptions o, Func<Task> next)
            {
                if (c.Request.Method.Equals(Constants.HttpVerbs.Post, StringComparison.OrdinalIgnoreCase) &&
                    c.Request.Headers.TryGetValue(o.MethodOverrideHeader, out var header))
                {
                    var value = header.ToString();
                    if (o.AllowedMethodOverrides.Contains(value, StringComparer.OrdinalIgnoreCase))
                    {
                        c.Request.Method = value;
                    }
                }

                await next();
            }
        }

        private static IApplicationBuilder UseResourceRewriting(this IApplicationBuilder app, bool snapshot)
        {
            if (snapshot)
            {
                return app.FeatureEnabled<ResourceRewritingOptions, PublicApiOptions>(out var options)
                    ? app.Use(async (context, next) => { await ExecuteFeature(context, options, next); })
                    : app;
            }

            return app.Use(async (context, next) =>
            {
                if (context.FeatureEnabled<ResourceRewritingOptions, PublicApiOptions>(out var options))
                {
                    await ExecuteFeature(context, options, next);
                }
            });

            async Task ExecuteFeature(HttpContext c, ResourceRewritingOptions o, Func<Task> next)
            {
                // Use X-Action to disambiguate one vs. many resources in a write call
                // See: http://restlet.com/blog/2015/05/18/implementing-bulk-updates-within-restful-services/
                var action = c.Request.Headers[o.ActionHeader];
                if (action.Count > 0)
                {
                    var path = c.Request.Path.ToUriComponent();
                    path = $"{path}/{action}";
                    c.Request.Path = path;
                }

                // Use 'application/merge-patch+json' header to disambiguate JSON patch strategy:
                // See: https://tools.ietf.org/html/rfc7386
                var contentType = c.Request.Headers[Constants.HttpHeaders.ContentType];
                if (contentType.Count > 0 && contentType.Contains(Constants.MediaTypes.JsonMergePatch))
                {
                    var path = c.Request.Path.ToUriComponent();
                    path = $"{path}/merge";
                    c.Request.Path = path;
                }

                await next();
            }
        }

        private static IApplicationBuilder UseRequestLimiting(this IApplicationBuilder app, bool snapshot)
        {
            if (snapshot)
            {
                return app.FeatureEnabled<RequestLimitOptions, PublicApiOptions>(out var options)
                    ? app.Use(async (context, next) => { await ExecuteFeature(context, options, next); })
                    : app;
            }

            return app.Use(async (context, next) =>
            {
                if (context.FeatureEnabled<RequestLimitOptions, PublicApiOptions>(out var options))
                {
                    await ExecuteFeature(context, options, next);
                }
            });

            async Task ExecuteFeature(HttpContext c, RequestLimitOptions o, Func<Task> next)
            {
                var bodySize = c.Features.Get<IHttpMaxRequestBodySizeFeature>();
                if (bodySize != null && !bodySize.IsReadOnly)
                {
                    bodySize.MaxRequestBodySize = o.MaxRequestSizeBytes;
                }

                await next();
            }
        }

        private static IApplicationBuilder UseJsonMultiCase(this IApplicationBuilder app, bool snapshot)
        {
            if (snapshot)
            {
                return app.FeatureEnabled<JsonConversionOptions, PublicApiOptions>(out var options)
                    ? app.Use(async (context, next) => { await ExecuteFeature(context, options, next); })
                    : app;
            }

            return app.Use(async (context, next) =>
            {
                if (context.FeatureEnabled<JsonConversionOptions, PublicApiOptions>(out var options))
                {
                    await ExecuteFeature(context, options, next);
                }
            });

            async Task ExecuteFeature(HttpContext c, JsonConversionOptions o, Func<Task> next)
            {
                var qs = c.Request.Query;
                qs.TryGetValue(o.MultiCaseOperator, out var values);
                foreach (var value in values)
                {
                    foreach (var entry in c.RequestServices.GetServices<ITextTransform>())
                    {
                        if (!entry.Name.Equals(value, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        c.Items[Constants.ContextKeys.JsonMultiCase] = entry;
                        goto next;
                    }
                }

                next:
                await next();
            }
        }
        
        #region Versioning

        // See: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#12-versioning

        public static IApplicationBuilder UseVersioning(this IApplicationBuilder app, bool snapshot = true)
        {
            if (snapshot)
            {
                return app.FeatureEnabled<VersioningOptions, PublicApiOptions>(out var options)
                    ? app.Use(async (context, next) => { await ExecuteFeature(context, options, next); })
                    : app;
            }

            return app.Use(async (context, next) =>
            {
                if (context.FeatureEnabled<VersioningOptions, PublicApiOptions>(out var options))
                {
                    await ExecuteFeature(context, options, next);
                }
            });

            async Task ExecuteFeature(HttpContext c, VersioningOptions o, Func<Task> next)
            {
                var versionResolver = c.RequestServices.GetRequiredService<IVersionContextResolver>();
                var versionContext = await versionResolver.ResolveAsync(c);
                if (versionContext != null && versionContext != VersionContext.None)
                {
                    c.SetVersionContext(versionContext);
                }
                else
                {
                    if (!o.RequireExplicitVersion || c.Request.Path.StartsWithAny(o.VersionAgnosticPaths, StringComparison.OrdinalIgnoreCase))
                    {
                        c.SetVersionContext(VersionContext.None);
                    }
                    else
                    {
                        c.Response.StatusCode = o.ExplicitVersionRequiredStatusCode;
                        return;
                    }
                }

                await next();
            }
        }

        #endregion


        #region MultiTenancy

        public static IApplicationBuilder UseMultiTenancy<TTenant>(this IApplicationBuilder app, bool snapshot = true) where TTenant : class, ITenant<string>, new()
        {
            return app.UseMultiTenancy<TTenant, string>(snapshot);
        }

        public static IApplicationBuilder UseMultiTenancy<TTenant, TKey>(this IApplicationBuilder app,
            bool snapshot = true) where TTenant : class, ITenant<TKey>, new()
        {
            if (snapshot)
            {
                return app.FeatureEnabled<MultiTenancyOptions, PublicApiOptions>(out var options)
                    ? app.Use(async (context, next) => { await ExecuteFeature(context, options, next); })
                    : app;
            }

            return app.Use(async (context, next) =>
            {
                if (context.FeatureEnabled<MultiTenancyOptions, PublicApiOptions>(out var options))
                {
                    await ExecuteFeature(context, options, next);
                }
            });

            async Task ExecuteFeature(HttpContext c, MultiTenancyOptions o, Func<Task> next)
            {
                var tenantResolver = c.RequestServices.GetRequiredService<ITenantContextResolver<TTenant>>();
                var tenantContext = await tenantResolver.ResolveAsync(c);
                if (tenantContext != null)
                {
                    c.SetTenantContext(tenantContext);
                }
                else
                {
                    if (!o.RequireTenant)
                    {
                        if (!string.IsNullOrWhiteSpace(o.DefaultTenantId) &&
                            !string.IsNullOrWhiteSpace(o.DefaultTenantName))
                        {
                            c.SetTenantContext(new TenantContext<TTenant>
                            {
                                Tenant = new TTenant
                                {
                                    Name = o.DefaultTenantName,
                                    Id = (TKey) (Convert.ChangeType(o.DefaultTenantId, typeof(TKey)) ?? default(TKey))
                                }
                            });
                        }
                    }
                    else
                    {
                        c.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return;
                    }
                }

                await next();
            }
        }

        #endregion
    }
}
