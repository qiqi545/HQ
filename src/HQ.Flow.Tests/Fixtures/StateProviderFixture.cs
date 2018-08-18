using System;
using System.Threading;

namespace HQ.Flow.Tests.Fixtures
{
	public class StateProviderFixture : IDisposable
	{
		private static readonly object Sync = new object();

		public StateProviderFixture()
		{
			Monitor.Enter(Sync);
			StateProvider.Clear();
		}

		public void Dispose()
		{
			StateProvider.Clear();
			Monitor.Exit(Sync);
		}
	}
}