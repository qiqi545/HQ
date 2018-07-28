using System;

namespace cadence.tests
{
	public class MetricsFixture : IDisposable
	{
		readonly MetricsInstance _instance;

		public MetricsInstance Metrics => _instance;

		public MetricsFixture()
		{
			_instance = new MetricsInstance();
		}

		public void Dispose()
		{
			_instance.Clear();
		}
	}
}