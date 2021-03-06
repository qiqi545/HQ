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

using ActiveRoutes;

namespace HQ.Integration.Azure.Metrics.Reporters
{
	public class AppInsightsMetricsReporterOptions : IFeatureToggle
	{
		public bool Enabled { get; set; } = true;
		public string HealthCheckEventName { get; set; } = Constants.Events.HealthCheck;
		public string MetricsSampleEventName { get; set; } = Constants.Events.MetricsSample;

		public bool PublishHealthy { get; set; } = false;
		public bool PublishHealthChecks { get; set; } = true;
		public bool PublishMetrics { get; set; } = true;
	}
}