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
using System.Collections.Immutable;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using HQ.Extensions.Metrics.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace HQ.Extensions.Metrics.AspNetCore.Internal
{
	internal class MetricsMiddleware
	{
		private readonly PathString _endpoint;
		private readonly RequestDelegate _next;
		private readonly IOptions<MetricsOptions> _options;

		public MetricsMiddleware(RequestDelegate next, PathString endpoint, IOptions<MetricsOptions> options)
		{
			_next = next;
			_endpoint = endpoint;
			_options = options;
			if (!_endpoint.Value.StartsWith("/"))
			{
				_endpoint = $"/{_endpoint}";
			}
		}

		public async Task Invoke(HttpContext context)
		{
			if (context.Request.Path == _endpoint)
			{
				await GetMetricsSample(context, TimeSpan.FromSeconds(_options.Value.SampleTimeoutSeconds),
					_options.Value.TypeFilter);
			}
			else
			{
				await _next.Invoke(context);
			}
		}

		public static async Task GetMetricsSample(HttpContext context, TimeSpan timeout, MetricType typeFilter)
		{
			var registry = context.RequestServices.GetRequiredService<IMetricsRegistry>();
			var samples = registry.SelectMany(x => x.GetSample(typeFilter)).ToImmutableDictionary();
			var cancel = new CancellationTokenSource(timeout);

			context.Response.StatusCode = 200;
			context.Response.Headers.Add(HeaderNames.ContentType, MediaTypeNames.Application.Json);
			await context.Response.WriteAsync(JsonSampleSerializer.Serialize(samples), cancel.Token);
		}
	}
}