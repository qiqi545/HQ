// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace HQ.Flow.Tests.Fakes
{
	public class LongRunningAsyncHandler : IConsume<IEvent>
	{
		public int Handled { get; private set; }

		public async Task<bool> HandleAsync(IEvent message)
		{
			await WaitAround();
			return true;
		}

		private async Task WaitAround()
		{
			await Task.Run(() =>
			{
				Thread.Sleep(TimeSpan.FromSeconds(2));
				Handled++;
			});
		}
	}
}