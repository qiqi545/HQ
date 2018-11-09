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
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HQ.Touchstone
{
    public abstract class SystemUnderTest : TestBase, ILogger, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;

        private ILogger<SystemUnderTest> _logger;

        protected SystemUnderTest(IServiceProvider serviceProvider = null)
        {
            _serviceProvider = serviceProvider;

            TryInstallLogging(serviceProvider);

            Trace.Listeners.Add(new ActionTraceListener(message =>
            {
                var outputProvider = AmbientContext.OutputProvider;
                if (outputProvider?.IsAvailable != true)
                    return;
                outputProvider.WriteLine(message);
            }));
        }

        private void TryInstallLogging(IServiceProvider serviceProvider)
        {
            var loggerFactory = serviceProvider?.GetService<ILoggerFactory>();
            loggerFactory = loggerFactory ?? defaultLoggerFactory;
            loggerFactory.AddProvider(CreateLoggerProvider());
            _logger = loggerFactory.CreateLogger<SystemUnderTest>();
        }

        #region ILogger

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var logger = GetLogger();
            logger?.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            var logger = GetLogger();
            return logger != null && logger.IsEnabled(logLevel);
        }
        
        public IDisposable BeginScope<TState>(TState state)
        {
            var logger = GetLogger();
            return logger?.BeginScope(state);
        }

        private ILogger<SystemUnderTest> GetLogger()
        {
            var logger = _serviceProvider?.GetService<ILogger<SystemUnderTest>>() ?? _logger;
            return logger;
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) { }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
