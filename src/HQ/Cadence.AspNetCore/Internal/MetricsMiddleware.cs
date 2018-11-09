// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HQ.Cadence.Internal;
using HQ.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Cadence.AspNetCore.Internal
{
	internal class MetricsMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly MetricsOptions _options;

		public MetricsMiddleware(RequestDelegate next, IOptions<MetricsOptions> options)
		{
			_next = next;
			_options = options.Value;

			if (!options.Value.Path.StartsWith("/"))
				options.Value.Path = "/" + options.Value.Path;
		}

		public async Task Invoke(HttpContext context)
		{
			if (context.Request.Path == _options.Path)
			{
				var registry = context.RequestServices.GetRequiredService<IMetricsRegistry>();
				var samples = registry.SelectMany(x => x.AsReadOnly);
				var cancel = new CancellationTokenSource(_options.Timeout);

				context.Response.StatusCode = 200;
				context.Response.Headers.Add(HttpHeaders.ContentType, MediaTypes.Json);
				await context.Response.WriteAsync(JsonSerializer.Serialize(samples), cancel.Token);
			}
			else
			{
				await _next.Invoke(context);
			}
		}
	}
}