using System;

namespace cadence.tests
{
	public class MetricsFixture : IDisposable
	{
		public void Dispose()
		{
			Metrics.Clear();
		}
	}
}