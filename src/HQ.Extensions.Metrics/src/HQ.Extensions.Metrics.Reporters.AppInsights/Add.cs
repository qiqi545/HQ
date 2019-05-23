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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Metrics.Reporters.AppInsights
{
    // https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-2.2

    public static class Add
    {
        /// <summary>
        /// Sends all metrics and health checks periodically to an Application Insights instrumentation.
        /// </summary>
        public static IMetricsBuilder PushToAppInsights(this IMetricsBuilder builder, IConfiguration config)
        {
            return builder.PushToAppInsights(config.Bind);
        }

        /// <summary>
        /// Sends all metrics and health checks periodically to an Application Insights instrumentation.
        /// </summary>
        public static IMetricsBuilder PushToAppInsights(this IMetricsBuilder builder, Action<AppInsightsMetricsReporterOptions> configureAction = null)
        {
            builder.Services.Configure<HealthCheckPublisherOptions>(options =>
            {
                options.Predicate = check => true;
                options.Delay = TimeSpan.FromSeconds(2);
                options.Timeout = TimeSpan.FromSeconds(30);
                options.Period = TimeSpan.FromSeconds(30);
            });
            builder.Services.Configure(configureAction);
            builder.Services.AddSingleton<IHealthCheckPublisher>(r => new AppInsightsMetricsPublisher(
                r.GetRequiredService<IMetricsRegistry>(),
                r.GetRequiredService<IOptionsMonitor<AppInsightsMetricsReporterOptions>>()
            ));

            // The following workaround permits adding an IHealthCheckPublisher 
            // instance to the service container when one or more other hosted 
            // services have already been added to the app. This workaround
            // won't be required with the release of ASP.NET Core 3.0. For more 
            // information, see: https://github.com/aspnet/Extensions/issues/639.
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton(typeof(IHostedService),
                    typeof(HealthCheckPublisherOptions).Assembly.GetType(
                        "Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckPublisherHostedService")));

            return builder;
        }
    }
}
