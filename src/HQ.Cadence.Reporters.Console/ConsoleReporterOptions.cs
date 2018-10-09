// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Cadence.Reporters.Console
{
	public class ConsoleReporterOptions
	{
		public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);
		public bool StopOnError { get; set; } = false;
	}
}