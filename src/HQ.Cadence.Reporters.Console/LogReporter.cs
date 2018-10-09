// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HQ.Cadence.Reporters.Console
{
	public sealed class LogReporter : PeriodicReporter
	{
		private readonly IMetricsRegistry _registry;
		private readonly IOptions<LogReporterOptions> _options;
		private readonly ILogger _logger;

		public LogReporter(IMetricsRegistry registry, ILoggerFactory loggerFactory, IOptions<LogReporterOptions> options) : base(options.Value.Interval)
		{
			_registry = registry;
			_options = options;
			_logger = loggerFactory.CreateLogger(options.Value.CategoryName);
		}

		public override void Report(CancellationToken? cancellationToken = null)
		{
			if (_logger == null || cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
				return;

			try
			{
				using (var stream = new MemoryStream())
				{
					var encoding = Encoding.UTF8;
					using (var writer = new StreamWriter(stream, encoding))
					{
						if (!ConsoleReporter.TryWrite(_registry, writer, cancellationToken) && _options.Value.StopOnError)
						{
							Stop();
							return;
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
		}
	}
}