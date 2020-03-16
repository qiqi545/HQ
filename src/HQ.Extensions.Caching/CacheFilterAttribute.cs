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

using ActiveCaching;
using HQ.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Extensions.Caching.AspNetCore.Mvc
{
	public class CacheFilterAttribute : ActionFilterAttribute
	{
		private readonly ICache _cache;

		public CacheFilterAttribute(ICache cache)
		{
			_cache = cache;
			Order = int.MaxValue;
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var keyBuilder = new CacheKeyBuilder(filterContext);
			var http = filterContext.HttpContext;
			var cacheKey = keyBuilder.Build();

			if (filterContext.ActionArguments.ContainsKey(Constants.ContextKeys.CacheArgument))
			{
				filterContext.ActionArguments[Constants.ContextKeys.CacheArgument] =
					http.RequestServices.GetService(typeof(ICache));
			}

			if (filterContext.ActionArguments.ContainsKey(Constants.ContextKeys.CacheKeyArgument))
			{
				filterContext.ActionArguments[Constants.ContextKeys.CacheKeyArgument] = cacheKey;
			}

			var existing = _cache.Get(cacheKey);
			if (existing is IActionResult result)
			{
				filterContext.Result = result;
				return;
			}

			http.Items.Add("cacheKey", cacheKey);
			base.OnActionExecuting(filterContext);
		}

		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			if (!(filterContext.HttpContext.Items[Constants.ContextKeys.CacheKeyArgument] is string key))
				return;

			var value = filterContext.Result;
			if (value != null)
				_cache.Set(key, value);

			base.OnResultExecuted(filterContext);
		}
	}
}