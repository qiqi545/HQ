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

using System.Threading;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Extensions.Logging;
using HQ.Extensions.Scheduling.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Scheduling.Models
{
    public class BackgroundTaskService : IHostedService
    {
        private readonly BackgroundTaskHost _host;

        public BackgroundTaskService(IServerTimestampService timestamps,
            IOptionsMonitor<BackgroundTaskSettings> settings, ISafeLogger<BackgroundTaskHost> logger)
        {
            _host = new BackgroundTaskHost(timestamps, settings, logger);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _host.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _host.Stop(cancellationToken);
            return Task.CompletedTask;
        }
    }
}
