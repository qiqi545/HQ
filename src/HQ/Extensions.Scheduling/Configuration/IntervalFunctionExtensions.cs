using System;

namespace HQ.Extensions.Scheduling.Configuration
{
	public static class IntervalFunctionExtensions
	{
		public static readonly Func<int, TimeSpan> ExponentialBackoff = i => TimeSpan.FromSeconds(5 + Math.Pow(i, 4));

		public static TimeSpan NextInterval(this IntervalFunction function, int attempts)
		{
			switch (function)
			{
				case IntervalFunction.ExponentialBackoff:
					return ExponentialBackoff(attempts);
				default:
					throw new ArgumentOutOfRangeException(nameof(function), function, null);
			}
		}
	}
}