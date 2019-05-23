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
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Extensions.Metrics.Reporters.SignalR
{
    public static class HubReporterExtensions
    {
        public static IMetricsBuilder AddHubReporter<T>(this IMetricsBuilder builder) where T : Hub
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetricsReporter, HubReporter<T>>());
            return builder;
        }

        public static IMetricsBuilder AddHubReporter<T>(this IMetricsBuilder builder,
            Action<HubReporterOptions> configureAction)
            where T : Hub
        {
            if (configureAction == null)
            {
                throw new ArgumentNullException(nameof(configureAction));
            }

            builder.AddHubReporter<T>();
            builder.Services.Configure(configureAction);

            return builder;
        }
    }
}
