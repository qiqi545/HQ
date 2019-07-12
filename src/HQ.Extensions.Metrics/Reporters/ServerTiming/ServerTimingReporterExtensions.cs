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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Extensions.Metrics.Reporters.ServerTiming
{
    public static class ServerTimingReporterExtensions
    {
        public static IMetricsBuilder AddServerTimingReporter(this IMetricsBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetricsReporter, ServerTimingReporter>());
            return builder;
        }

        public static IMetricsBuilder AddServerTimingReporter(this IMetricsBuilder builder,
            Action<ServerTimingReporterOptions> configureAction)
        {
            if (configureAction == null)
            {
                throw new ArgumentNullException(nameof(configureAction));
            }

            builder.AddServerTimingReporter();
            builder.Services.Configure(configureAction);

            return builder;
        }

        public static IApplicationBuilder UseServerTimingReporter(this IApplicationBuilder app)
        {
            ServerTimingReporter.AppBuilder = app;
            return app;
        }
    }
}
