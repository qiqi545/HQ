// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace HQ.Cadence.Reporters
{
	public abstract class PeriodicReporter : IMetricsReporter
	{
		private readonly TimeSpan _interval;
		private CancellationTokenSource _cancel;
		private Task _task;

		protected PeriodicReporter(TimeSpan? interval)
		{
			_interval = interval ?? TimeSpan.FromSeconds(5);
		}

		public Task Start()
		{
			_cancel = new CancellationTokenSource();
			_task = OnTimer(() => Report(_cancel.Token), _cancel.Token);
			return _task;
		}

		public void Dispose()
		{
			Stop();
		}

		public abstract void Report(CancellationToken? cancellationToken = null);

		public async Task OnTimer(Action action, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(_interval, cancellationToken);

				if (!cancellationToken.IsCancellationRequested)
					action();
			}
		}

		public void Stop()
		{
			if (_task.IsCompleted)
			{
				_task?.Dispose();
			}
			else
			{
				_cancel.Cancel();
				_task?.Dispose();
				_cancel.Dispose();
			}
		}
	}
}