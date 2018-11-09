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
using System.Net.Http;
using HQ.Remix;
using HQ.Touchstone.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.TraceSource;

namespace HQ.Touchstone
{
    public abstract class SystemUnderTest<T> : TestScope, ILogger<T>, IDisposable where T : class
    {
        private readonly WebHostFixture<T> _systemUnderTest;
        private ILogger<SystemUnderTest<T>> _logger;

        private IServiceProvider _serviceProvider;

        protected SystemUnderTest()
        {
            _systemUnderTest = new WebHostFixture<T>(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Configuration(IConfiguration config)
        {
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            using (var resolver = new TypeResolver())
            {
                TryInstallShouldAssertions(resolver);
            }
        }

        public virtual void Configure(IApplicationBuilder app)
        {
            _serviceProvider = app.ApplicationServices;

            TryInstallLogging();

            TryInstallTracing();
        }

        public HttpClient CreateClient()
        {
            return _systemUnderTest.CreateClient();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _systemUnderTest?.Dispose();
        }

        private void TryInstallLogging()
        {
            var loggerFactory = _serviceProvider.GetService<ILoggerFactory>();
            loggerFactory = loggerFactory ?? defaultLoggerFactory;
            loggerFactory.AddProvider(CreateLoggerProvider());

            _logger = loggerFactory.CreateLogger<SystemUnderTest<T>>();
        }

        private void TryInstallTracing()
        {
            if (_serviceProvider.GetService<TraceSourceLoggerProvider>() != null)
                return;

            Trace.Listeners.Add(new ActionTraceListener(message =>
            {
                var outputProvider = AmbientContext.OutputProvider;
                if (outputProvider?.IsAvailable != true)
                    return;
                outputProvider.WriteLine(message);
            }));
        }

        #region ILogger<T>

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
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

        private ILogger<SystemUnderTest<T>> GetLogger()
        {
            var logger = _serviceProvider.GetService<ILogger<SystemUnderTest<T>>>() ?? _logger;
            return logger;
        }

        #endregion
    }
}
