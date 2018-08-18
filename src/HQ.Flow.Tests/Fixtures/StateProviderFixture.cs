using System;
using System.Threading;

namespace HQ.Flow.Tests.Fixtures
{
	public class StateProviderFixture : IDisposable
	{
		private static readonly object _sync = new object();

		public StateProviderFixture()
		{
			Monitor.Enter(_sync);
			StateProvider.Clear();
		}

		public void Dispose()
		{
			StateProvider.Clear();
			Monitor.Exit(_sync);
		}
	}
}