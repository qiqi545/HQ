// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Linq;
using HQ.Common;
using HQ.Common.Extensions;
using HQ.Domicile.Configuration;
using HQ.Domicile.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Domicile
{
	public static class Use
	{
		public static IApplicationBuilder UsePublicApi(this IApplicationBuilder app)
		{
			Bootstrap.EnsureInitialized();

			app.UseCors();
			app.UseResponseCompression();
			app.UseMethodOverrides();
			app.UseResourceRewriting();
			app.UseRequestLimiting();
			app.UseJsonMultiCase();

			return app;
		}

		private static IApplicationBuilder UseMethodOverrides(this IApplicationBuilder app)
		{
			return app.Use(async (context, next) =>
			{
				if (context.FeatureEnabled<PublicApiOptions.MethodOverrideOptions, PublicApiOptions>(out var feature))
					if (context.Request.Method.Equals(HttpVerbs.Post, StringComparison.OrdinalIgnoreCase) &&
					    context.Request.Headers.TryGetValue(feature.MethodOverrideHeader, out var header))
					{
						var value = header.ToString();
						if (feature.AllowedMethodOverrides.Contains(value, StringComparer.OrdinalIgnoreCase))
							context.Request.Method = value;
					}

				await next();
			});
		}

		private static IApplicationBuilder UseResourceRewriting(this IApplicationBuilder app)
		{
			return app.Use(async (context, next) =>
			{
				if (context.FeatureEnabled<PublicApiOptions.ResourceRewritingOptions, PublicApiOptions>(out var feature)
				)
				{
					// Use X-Action to disambiguate one vs. many resources in a write call
					// See: http://restlet.com/blog/2015/05/18/implementing-bulk-updates-within-restful-services/
					var action = context.Request.Headers[feature.ActionHeader];
					if (action.Count > 0)
					{
						var path = context.Request.Path.ToUriComponent();
						path = $"{path}/{action}";
						context.Request.Path = path;
					}

					// Use 'application/merge-patch+json' header to disambiguate JSON patch strategy:
					// See: https://tools.ietf.org/html/rfc7386
					var contentType = context.Request.Headers[HttpHeaders.ContentType];
					if (contentType.Count > 0 && contentType.Contains(MediaTypes.JsonMergePatch))
					{
						var path = context.Request.Path.ToUriComponent();
						path = $"{path}/merge";
						context.Request.Path = path;
					}
				}

				await next();
			});
		}

		private static IApplicationBuilder UseRequestLimiting(this IApplicationBuilder app)
		{
			return app.Use(async (context, next) =>
			{
				if (context.FeatureEnabled<PublicApiOptions.RequestLimitOptions, PublicApiOptions>(out var feature))
				{
					var bodySize = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
					if (bodySize != null && !bodySize.IsReadOnly)
						bodySize.MaxRequestBodySize = feature.MaxRequestSizeBytes;
				}

				await next();
			});
		}

		private static IApplicationBuilder UseJsonMultiCase(this IApplicationBuilder app)
		{
			return app.Use(async (context, next) =>
			{
				if (context.FeatureEnabled<PublicApiOptions.JsonMultiCaseOptions, PublicApiOptions>(out var feature))
				{
					var qs = context.Request.Query;
					qs.TryGetValue(feature.QueryStringParameter, out var values);

					foreach (var value in values)
					foreach (var entry in context.RequestServices.GetServices<ITextTransform>())
					{
						if (!entry.Name.Equals(value, StringComparison.OrdinalIgnoreCase))
							continue;
						context.Items[HqContextKeys.JsonMultiCase] = entry;
						break;
					}

					await next();
				}
			});
		}
	}
}