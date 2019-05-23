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
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HQ.Extensions.Metrics.Reporters.Console;
using HQ.Extensions.Metrics.Reporting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Metrics.Reporters.Logging
{
    public sealed class LogReporter : PeriodicReporter
    {
        private readonly ILogger _logger;
        private readonly IOptions<LogReporterOptions> _options;
        private readonly IMetricsRegistry _registry;

        public LogReporter(IMetricsRegistry registry, ILoggerFactory loggerFactory,
            IOptions<LogReporterOptions> options) : base(options.Value.Interval)
        {
            _registry = registry;
            _options = options;
            _logger = loggerFactory.CreateLogger(options.Value.CategoryName);
        }
        
        public override Task Report(CancellationToken cancellationToken = default)
        {
            if (_logger == null || cancellationToken.IsCancellationRequested)
                return Task.CompletedTask;

            try
            {
                using (var stream = new MemoryStream())
                {
                    var encoding = Encoding.UTF8;
                    using (var writer = new StreamWriter(stream, encoding))
                    {
                        if (!ConsoleReporter.TryWrite(_registry, writer, cancellationToken) &&
                            _options.Value.StopOnError)
                        {
                            Stop();
                            return Task.CompletedTask;
                        }
                    }

                    _logger.Log(_options.Value.LogLevel, encoding.GetString(stream.ToArray()));
                }
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Error reporting metrics to logger");
                if (_options.Value.StopOnError)
                    Stop();
            }

            return Task.CompletedTask;
        }

    }
}
