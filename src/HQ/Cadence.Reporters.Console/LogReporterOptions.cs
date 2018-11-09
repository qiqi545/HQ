// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using Microsoft.Extensions.Logging;

namespace HQ.Cadence.Reporters.Console
{
	public class LogReporterOptions
	{
		public string CategoryName { get; set; } = "HQ.Cadence";
		public LogLevel LogLevel { get; set; } = LogLevel.Information;
		public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);
		public bool StopOnError { get; set; } = false;
	}
}