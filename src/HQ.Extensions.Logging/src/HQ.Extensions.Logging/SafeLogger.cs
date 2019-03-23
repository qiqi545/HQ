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
using Microsoft.Extensions.Logging;

namespace HQ.Extensions.Logging
{
    public class SafeLogger : ISafeLogger
    {
        private readonly ILogger _inner;

        public SafeLogger(ILogger inner)
        {
            _inner = inner;
        }

        public void Trace(Func<string> message)
        {
            if (_inner.IsEnabled(LogLevel.Trace))
                _inner.LogTrace(message());
        }

        public void Trace(Func<string> message, Exception exception)
        {
            if (_inner.IsEnabled(LogLevel.Trace))
                _inner.LogTrace(exception, message());
        }

        public void Debug(Func<string> message)
        {
            if (_inner.IsEnabled(LogLevel.Debug))
                _inner.LogDebug(message());
        }

        public void Debug(Func<string> message, Exception exception)
        {
            if (_inner.IsEnabled(LogLevel.Debug))
                _inner.LogDebug(exception, message());
        }

        public void Info(Func<string> message)
        {
            if (_inner.IsEnabled(LogLevel.Information))
                _inner.LogInformation(message());
        }

        public void Info(Func<string> message, Exception exception)
        {
            if (_inner.IsEnabled(LogLevel.Information))
                _inner.LogInformation(exception, message());
        }

        public void Warn(Func<string> message)
        {
            if (_inner.IsEnabled(LogLevel.Warning))
                _inner.LogWarning(message());
        }

        public void Warn(Func<string> message, Exception exception)
        {
            if (_inner.IsEnabled(LogLevel.Warning))
                _inner.LogWarning(exception, message());
        }

        public void Error(Func<string> message)
        {
            if (_inner.IsEnabled(LogLevel.Error))
                _inner.LogError(message());
        }

        public void Error(Func<string> message, Exception exception)
        {
            if (_inner.IsEnabled(LogLevel.Error))
                _inner.LogError(exception, message());
        }

        public void Critical(Func<string> message)
        {
            if (_inner.IsEnabled(LogLevel.Critical))
                _inner.LogCritical(message());
        }

        public void Critical(Func<string> message, Exception exception)
        {
            if (_inner.IsEnabled(LogLevel.Critical))
                _inner.LogCritical(exception, message());
        }

        #region ILogger

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            _inner.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _inner.IsEnabled(logLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _inner.BeginScope(state);
        }

        #endregion
    }
}
