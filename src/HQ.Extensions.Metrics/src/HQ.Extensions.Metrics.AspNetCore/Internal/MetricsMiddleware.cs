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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HQ.Extensions.Metrics.Internal;
using HQ.Common;
using HQ.Extensions.Metrics.AspNetCore.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Metrics.AspNetCore.Internal
{
    internal class MetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<MetricsOptions> _options;
        private readonly IOptions<MetricsMiddlewareOptions> _middlewareOptions;

        public MetricsMiddleware(RequestDelegate next,
            IOptions<MetricsOptions> options,
            IOptions<MetricsMiddlewareOptions> middlewareOptions)
        {
            _next = next;
            _options = options;
            _middlewareOptions = middlewareOptions;
            if (!_middlewareOptions.Value.Path.StartsWith("/"))
                 _middlewareOptions.Value.Path = $"/{middlewareOptions.Value.Path}";
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == _middlewareOptions.Value.Path)
            {
                var registry = context.RequestServices.GetRequiredService<IMetricsRegistry>();
                var samples = registry.SelectMany(x => x.GetSample(_options.Value.Filter));
                var cancel = new CancellationTokenSource(_middlewareOptions.Value.Timeout);

                context.Response.StatusCode = 200;
                context.Response.Headers.Add(Constants.HttpHeaders.ContentType, Constants.MediaTypes.Json);
                await context.Response.WriteAsync(JsonSampleSerializer.Serialize(samples), cancel.Token);
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}
