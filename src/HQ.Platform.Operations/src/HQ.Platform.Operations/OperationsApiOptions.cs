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

using HQ.Common;
using HQ.Extensions.Metrics;
using Constants = HQ.Common.Constants;

namespace HQ.Platform.Operations
{
    public class OperationsApiOptions
    {
        public string RootPath { get; set; } = "/ops";

        public bool EnableRouteDebugging { get; set; } = true;
        public string RouteDebuggingPath { get; set; } = "/routes";

        public bool EnableEnvironmentEndpoint { get; set; } = true;
        public string EnvironmentEndpointPath { get; set; } = "/env";

        public bool EnableOptionsDebugging { get; set; } = true;
        public string OptionsDebuggingPath { get; set; } = "/options";

        public bool EnableServicesDebugging { get; set; } = true;
        public string ServicesDebuggingPath { get; set; } = "/services";

        public bool EnableFeatureDebugging { get; set; } = true;
        public string FeatureDebuggingPath { get; set; } = "/features";

        public bool EnableCacheDebugging { get; set; } = true;
        public string CacheDebuggingPath { get; set; } = "/caches";

        public bool EnableRequestProfiling { get; set; } = true;
        public string RequestProfilingHeader { get; set; } = Constants.HttpHeaders.ServerTiming;

        public bool EnableHealthChecksEndpoints { get; set; } = true;
        public string HealthCheckLivePath { get; set; } = "/ping";
        public string HealthChecksPath { get; set; } = "/health";

        public bool EnableMetricsEndpoint { get; set; } = true;
        public string MetricsEndpointPath { get; set; } = "/metrics";
        public MetricsOptions MetricsOptions { get; set; } = new MetricsOptions();
    }
}
