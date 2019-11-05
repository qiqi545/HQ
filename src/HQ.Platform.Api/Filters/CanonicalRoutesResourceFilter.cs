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
using System.Text;
using HQ.Common;
using HQ.Platform.Api.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.Platform.Api.Filters
{
	/// <summary>
	///     Compares externally source routing based on existing <see cref="RouteOptions" /> and permanently redirects when
	///     they differ.
	///     <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-2.2" />
	/// </summary>
	public class CanonicalRoutesResourceFilter : IResourceFilter
	{
		private const string SchemeDelimiter = "://";
		private const char ForwardSlash = '/';

		private readonly IOptions<ApiOptions> _options;

		public CanonicalRoutesResourceFilter(IOptions<ApiOptions> options) => _options = options;

		public void OnResourceExecuting(ResourceExecutingContext context)
		{
			if (!string.Equals(context.HttpContext.Request.Method, HttpMethods.Get,
				StringComparison.OrdinalIgnoreCase))
				return;

			if (!TryGetCanonicalRoute(context.HttpContext.Request, _options.Value.CanonicalRoutes,
				out var redirectToUrl))
				context.Result = new RedirectResult(redirectToUrl, true);
		}

		public void OnResourceExecuted(ResourceExecutedContext context)
		{
			/* Opportunity to display bad URLs in dashboard */
		}

		internal static bool TryGetCanonicalRoute(HttpRequest request, CanonicalRoutesOptions options,
			out string redirectToUrl)
		{
			var canonical = true;

			var appendTrailingSlash = options.AppendTrailingSlash;
			var lowercaseUrls = options.LowercaseUrls;
			var lowercaseQueryStrings = options.LowercaseQueryStrings;

			var sb = Pooling.StringBuilderPool.Get();
			try
			{
				if (lowercaseUrls)
				{
					AppendLowercase(sb, request.Scheme, ref canonical);
				}
				else
				{
					sb.Append(request.Scheme);
				}

				sb.Append(SchemeDelimiter);

				if (request.Host.HasValue)
				{
					if (lowercaseUrls)
					{
						AppendLowercase(sb, request.Host.Value, ref canonical);
					}
					else
					{
						sb.Append(request.Host);
					}
				}

				if (request.PathBase.HasValue)
				{
					if (lowercaseUrls)
					{
						AppendLowercase(sb, request.PathBase.Value, ref canonical);
					}
					else
					{
						sb.Append(request.PathBase);
					}

					if (appendTrailingSlash && !request.Path.HasValue)
					{
						if (request.PathBase.Value[request.PathBase.Value.Length - 1] != ForwardSlash)
						{
							sb.Append(ForwardSlash);
							canonical = false;
						}
					}
				}

				if (request.Path.HasValue)
				{
					if (lowercaseUrls)
					{
						AppendLowercase(sb, request.Path.Value, ref canonical);
					}
					else
					{
						sb.Append(request.Path);
					}

					if (appendTrailingSlash)
					{
						if (request.Path.Value[request.Path.Value.Length - 1] != ForwardSlash)
						{
							sb.Append(ForwardSlash);
							canonical = false;
						}
					}
				}

				if (request.QueryString.HasValue)
				{
					if (lowercaseUrls && lowercaseQueryStrings)
					{
						AppendLowercase(sb, request.QueryString.Value, ref canonical);
					}
					else
					{
						sb.Append(request.QueryString);
					}
				}

				redirectToUrl = canonical ? null : sb.ToString();
			}
			finally
			{
				Pooling.StringBuilderPool.Return(sb);
			}

			return canonical;
		}

		private static void AppendLowercase(StringBuilder sb, string value, ref bool valid)
		{
			for (var i = 0; i < value.Length; i++)
			{
				if (char.IsUpper(value, i))
				{
					valid = false;
					sb.Append(char.ToLowerInvariant(value[i]));
				}
				else
				{
					sb.Append(value[i]);
				}
			}
		}
	}
}