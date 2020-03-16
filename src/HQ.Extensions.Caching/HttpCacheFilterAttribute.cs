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
using System.Net;
using System.Text;
using ActiveCaching;
using HQ.Data.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace HQ.Extensions.Caching
{
	public class HttpCacheFilterAttribute : ActionFilterAttribute
	{
		private readonly IHttpCache _cache;
		private readonly IETagGenerator _generator;
		private readonly JsonSerializerSettings _settings;

		public HttpCacheFilterAttribute(IETagGenerator generator, IHttpCache cache, JsonSerializerSettings settings)
		{
			_generator = generator;
			_cache = cache;
			_settings = settings;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var request = context.HttpContext.Request;
			if (request.Method == HttpMethods.Get)
			{
				HandleSafeRequests(context, request);
			}
			else
			{
				HandleUnsafeRequests(context, request);
			}
		}

		private void HandleUnsafeRequests(ActionExecutingContext context, HttpRequest request)
		{
			var key = request.GetDisplayUrl();

			if (IfMatchFailed(request, key) || UnmodifiedSinceFailed(request, key))
			{
				context.Result = new StatusCodeResult((int) HttpStatusCode.PreconditionFailed);
			}
		}

		private void HandleSafeRequests(ActionExecutingContext context, HttpRequest request)
		{
			var key = request.GetDisplayUrl();

			if (NoneMatch(request, key) || NotModifiedSince(request, key))
			{
				context.Result = new StatusCodeResult((int) HttpStatusCode.NotModified);
			}
		}

		public override void OnActionExecuted(ActionExecutedContext context)
		{
			base.OnActionExecuted(context);

			MaybeCacheObject(context);
		}

		private void MaybeCacheObject(ActionExecutedContext context)
		{
			if (!(context.Result is OkObjectResult result))
			{
				return;
			}

			var cacheKey = context.HttpContext.Request.GetDisplayUrl();
			var json = JsonConvert.SerializeObject(result.Value, _settings);
			var etag = _generator.GenerateFromBuffer(Encoding.UTF8.GetBytes(json));
			context.HttpContext.Response.Headers.Add(HeaderNames.ETag, new[] {etag});
			_cache.Save(cacheKey, etag);

			if (result.Value is IObject resource)
			{
				var lastModifiedDate = resource.CreatedAt;
				context.HttpContext.Response.Headers.Add(HeaderNames.LastModified,
					lastModifiedDate.ToString("R"));
				_cache.Save(cacheKey, lastModifiedDate);
			}
		}

		#region State Probes

		private bool NoneMatch(HttpRequest request, string key)
		{
			return request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var ifNoneMatch) &&
			       _cache.TryGetETag(key, out var etag) && ifNoneMatch == etag;
		}

		private bool IfMatchFailed(HttpRequest request, string key)
		{
			return request.Headers.TryGetValue(HeaderNames.IfMatch, out var ifMatch) &&
			       _cache.TryGetETag(key, out var etag) && ifMatch != etag;
		}

		private bool UnmodifiedSinceFailed(HttpRequest request, string key)
		{
			return request.Headers.TryGetValue(HeaderNames.IfUnmodifiedSince, out var ifUnmodifiedSince) &&
			       DateTimeOffset.TryParse(ifUnmodifiedSince, out var ifUnmodifiedSinceDate) &&
			       _cache.TryGetLastModified(key, out var lastModifiedDate) && lastModifiedDate > ifUnmodifiedSinceDate;
		}

		private bool NotModifiedSince(HttpRequest request, string key)
		{
			return request.Headers.TryGetValue(HeaderNames.IfModifiedSince, out var ifModifiedSince) &&
			       DateTimeOffset.TryParse(ifModifiedSince, out var ifModifiedSinceDate)
			       && _cache.TryGetLastModified(key, out var lastModifiedDate) &&
			       lastModifiedDate <= ifModifiedSinceDate;
		}

		#endregion
	}
}