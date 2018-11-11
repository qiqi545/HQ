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
