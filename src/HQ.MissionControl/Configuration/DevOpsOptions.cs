// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using HQ.Common;

namespace HQ.MissionControl.Configuration
{
	public class DevOpsOptions
	{
		public string RootPath { get; set; } = "/ops";

		public bool EnableRouteDebugging { get; set; } = true;
		public string RouteDebuggingPath { get; set; } = "/routes";

		public bool EnableRequestProfiling { get; set; } = true;
		public string RequestProfilingHeader { get; set; } = HqHeaders.ExecutionTimeMs;

		public bool EnableEnvironmentEndpoint { get; set; } = true;
		public string EnvironmentEndpointPath { get; set; } = "/env";
	}
}