#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

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

        public Task InitializeAsync()
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
