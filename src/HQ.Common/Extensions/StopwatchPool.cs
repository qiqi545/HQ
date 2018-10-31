using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis.PooledObjects;

namespace HQ.Common.Extensions
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