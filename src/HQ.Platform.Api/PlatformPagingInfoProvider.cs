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

using ActiveStorage;
using ActiveText;
using HQ.Data.Contracts.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.Platform.Api.Extensions
{
	public sealed class PlatformPagingInfoProvider : IPagingInfoProvider
	{
		private readonly IOptionsMonitor<QueryOptions> _options;

		public PlatformPagingInfoProvider(IOptionsMonitor<QueryOptions> options)
		{
			_options = options;
		}

		internal static string GetPreviousPage(HttpRequest request, IPageHeader header, QueryOptions options)
		{
			return !header.HasPreviousPage
				? null
				: $"{request.Scheme}://{request.Host}{request.Path}?{options.PageOperator}={header.Index - 1}&{options.PerPageOperator}={header.Size}";
		}

		internal static string GetNextPage(HttpRequest request, IPageHeader header, QueryOptions options)
		{
			return !header.HasNextPage
				? null
				: $"{request.Scheme}://{request.Host}{request.Path}?{options.PageOperator}={header.Index + 2}&{options.PerPageOperator}={header.Size}";
		}

		internal static string GetFirstPage(HttpRequest request, IPageHeader header, QueryOptions options)
		{
			return header.TotalPages == 0
				? null
				: $"{request.Scheme}://{request.Host}{request.Path}?{options.PageOperator}=1&{options.PerPageOperator}={header.Size}";
		}

		internal static string GetLastPage(HttpRequest request, IPageHeader header, QueryOptions options)
		{
			return header.TotalPages < 2
				? null
				: $"{request.Scheme}://{request.Host}{request.Path}?{options.PageOperator}={header.TotalPages}&{options.PerPageOperator}={header.Size}";
		}

		#region Implementation of IPagingInfoProvider

		public PagingInfo GetPagingInfo<T>(HttpRequest request, IPage<T> data) =>
			new PagingInfo
			{
				TotalCount = data.TotalCount,
				TotalPages = data.TotalPages,
				NextPage = GetNextPage(request, data, _options.CurrentValue),
				PreviousPage = GetPreviousPage(request, data, _options.CurrentValue),
				LastPage = GetLastPage(request, data, _options.CurrentValue)
			};

		public void SetPagingInfoHeaders<T>(HttpRequest request, HttpResponse response, IPage<T> data)
		{
			var link = Pooling.StringBuilderPool.Scoped(sb =>
			{
				if (data.TotalPages > 1)
				{
					var firstPage = $"<{GetFirstPage(request, data, _options.CurrentValue)}>; rel=\"first\"";
					sb.Append(firstPage);
				}

				if (data.HasNextPage)
				{
					var nextPage = $"<{GetNextPage(request, data, _options.CurrentValue)}>; rel=\"next\"";
					if (sb.Length > 0)
					{
						sb.Append(", ");
					}

					sb.Append(nextPage);
				}

				if (data.HasPreviousPage)
				{
					var previousPage = $"<{GetPreviousPage(request, data, _options.CurrentValue)}>; rel=\"previous\"";
					if (sb.Length > 0)
					{
						sb.Append(", ");
					}

					sb.Append(previousPage);
				}

				if (data.TotalPages > 1)
				{
					var lastPage = $"<{GetLastPage(request, data, _options.CurrentValue)}>; rel=\"last\"";
					if (sb.Length > 0)
					{
						sb.Append(", ");
					}

					sb.Append(lastPage);
				}
			});

			if (link.Length > 0)
			{
				response.Headers.Add(HttpHeaders.Link, link);
			}

			response.Headers.Add(_options.CurrentValue.TotalCountHeader, data.TotalCount.ToString());
			response.Headers.Add(_options.CurrentValue.TotalPagesHeader, data.TotalPages.ToString());
		}

		#endregion
	}
}