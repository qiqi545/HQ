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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HQ.Extensions.Metrics
{
    public interface IMetricsBuilder
    {
        /// <summary>
        ///     Gets the <see cref="IServiceCollection" /> where metrics services are configured.
        /// </summary>
        IServiceCollection Services { get; }

        IMetricsBuilder RegisterAsHealthCheck(Func<IMetricsHost, GaugeMetric<bool>> builderFunc, HealthStatus onCheckFailure = HealthStatus.Unhealthy);
        IMetricsBuilder RegisterAsHealthCheck<T>(Func<IMetricsHost, GaugeMetric<T>> builderFunc, Func<T, bool> checkFunc, HealthStatus onCheckFailure = HealthStatus.Unhealthy);
        IMetricsBuilder RegisterAsHealthCheck(Func<IMetricsHost, CounterMetric> builderFunc, Func<long, bool> checkFunc, HealthStatus onCheckFailure = HealthStatus.Unhealthy);
        IMetricsBuilder RegisterAsHealthCheck<TMetric, TValue>(Func<IMetricsHost, TMetric> builderFunc, Func<TMetric, TValue> valueFunc, Func<TValue, bool> checkFunc, HealthStatus onCheckFailure = HealthStatus.Unhealthy) where TMetric : IMetric;
    }
}
