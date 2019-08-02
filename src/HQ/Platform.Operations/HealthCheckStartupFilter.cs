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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Operations
{
    /// <summary>
    ///     Fails startup if health checks fail. This is a fat canary.
    /// </summary>
    public class HealthCheckStartupFilter : IStartupFilter
    {
        private readonly IOptionsMonitor<HealthCheckOptions> _options;
        private readonly HealthCheckService _service;

        public HealthCheckStartupFilter(HealthCheckService service, IOptionsMonitor<HealthCheckOptions> options)
        {
            _options = options;
            _service = service;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            var report = _service.CheckHealthAsync(r => r.Tags.Contains("startup")).GetAwaiter().GetResult();

            if (report.Status == HealthStatus.Unhealthy)
            {
                throw new Exception("Application failed to start due to failing startup health checks.");
            }

            return next;
        }
    }
}
