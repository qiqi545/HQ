// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis.PooledObjects;

namespace HQ.Common.Helpers
{
	public static class StopwatchPool
	{
		public static TimeSpan Scoped(Action<Stopwatch> closure)
		{
			var sw = PooledStopwatch.StartInstance();
			closure(sw);
			var elapsed = sw.Elapsed;
			sw.Free();
			return elapsed;
		}
	}
}