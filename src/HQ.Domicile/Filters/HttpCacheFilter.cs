// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Net;
using System.Text;
using HQ.Common;
using HQ.Domicile.Models;
using HQ.Rosetta;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace HQ.Domicile.Filters
{
	public class HttpCacheFilter : ActionFilterAttribute
	{
		private readonly IHttpCache _cache;
		private readonly IETagGenerator _generator;
		private readonly JsonSerializerSettings _settings;

		public HttpCacheFilter(IETagGenerator generator, IHttpCache cache, JsonSerializerSettings settings)
		{
			_generator = generator;
			_cache = cache;
			_settings = settings;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var request = context.HttpContext.Request;
			if (request.Method == HttpVerbs.Get)
				HandleSafeRequests(context, request);
			else
				HandleUnsafeRequests(context, request);
		}

		private void HandleUnsafeRequests(ActionExecutingContext context, HttpRequest request)
		{
			var key = request.GetDisplayUrl();

			if (IfMatchFailed(request, key) || UnmodifiedSinceFailed(request, key))
				context.Result = new StatusCodeResult((int) HttpStatusCode.PreconditionFailed);
		}

		private void HandleSafeRequests(ActionExecutingContext context, HttpRequest request)
		{
			var key = request.GetDisplayUrl();

			if (NoneMatch(request, key) || NotModifiedSince(request, key))
				context.Result = new StatusCodeResult((int) HttpStatusCode.NotModified);
		}
		
		public override void OnActionExecuted(ActionExecutedContext context)
		{
			base.OnActionExecuted(context);

			MaybeCacheObject(context);
		}

		private void MaybeCacheObject(ActionExecutedContext context)
		{
			if (!(context.Result is OkObjectResult result))
				return;

			var cacheKey = context.HttpContext.Request.GetDisplayUrl();
			var json = JsonConvert.SerializeObject(result.Value, _settings);
			var etag = _generator.GenerateFromBuffer(Encoding.UTF8.GetBytes(json));

			context.HttpContext.Response.Headers.Add(HttpHeaders.ETag, new[] {etag});
			_cache.Save(cacheKey, etag);

			// HQ.Rosetta: auto-enrich results with Last-Modified.
			if (result.Value is IObject resource)
			{
				var lastModifiedDate = resource.CreatedAt;
				context.HttpContext.Response.Headers.Add(HttpHeaders.LastModified, lastModifiedDate.ToString("R"));
				_cache.Save(cacheKey, lastModifiedDate);
			}
		}

		#region State Probes

		private bool NoneMatch(HttpRequest request, string key)
		{
			return request.Headers.TryGetValue(HttpHeaders.IfNoneMatch, out var ifNoneMatch) &&
			       _cache.TryGetETag(key, out var etag) && ifNoneMatch == etag;
		}

		private bool IfMatchFailed(HttpRequest request, string key)
		{
			return request.Headers.TryGetValue(HttpHeaders.IfMatch, out var ifMatch) &&
			       _cache.TryGetETag(key, out var etag) && ifMatch != etag;
		}

		private bool UnmodifiedSinceFailed(HttpRequest request, string key)
		{
			return request.Headers.TryGetValue(HttpHeaders.IfUnmodifiedSince, out var ifUnmodifiedSince) &&
			       DateTimeOffset.TryParse(ifUnmodifiedSince, out var ifUnmodifiedSinceDate) &&
			       _cache.TryGetLastModified(key, out var lastModifiedDate) && lastModifiedDate > ifUnmodifiedSinceDate;
		}

		private bool NotModifiedSince(HttpRequest request, string key)
		{
			return request.Headers.TryGetValue(HttpHeaders.IfModifiedSince, out var ifModifiedSince) &&
			       DateTimeOffset.TryParse(ifModifiedSince, out var ifModifiedSinceDate)
			       && _cache.TryGetLastModified(key, out var lastModifiedDate) &&
			       lastModifiedDate <= ifModifiedSinceDate;
		}

		#endregion
	}
}