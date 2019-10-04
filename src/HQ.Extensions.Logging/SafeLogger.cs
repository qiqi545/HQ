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
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace HQ.Extensions.Logging
{
	public class SafeLogger : ISafeLogger
	{
		private readonly ILogger _inner;
		private readonly IEnumerable<ISafeLoggerInterceptor> _interceptors;
		private int _safe;

		public SafeLogger(ILogger inner, IEnumerable<ISafeLoggerInterceptor> interceptors)
		{
			_inner = inner;
			_interceptors = interceptors;
		}

		public SafeLogger(ILogger inner, params ISafeLoggerInterceptor[] interceptors)
		{
			_inner = inner;
			_interceptors = interceptors;
		}

		public void Trace(Func<string> message, params object[] args)
		{
			SafeLog(LogLevel.Trace, message, args);
		}

		public void Trace(Func<string> message, Exception exception, params object[] args)
		{
			SafeLog(LogLevel.Trace, message, exception, args);
		}

		public void Debug(Func<string> message, params object[] args)
		{
			SafeLog(LogLevel.Debug, message, args);
		}

		public void Debug(Func<string> message, Exception exception, params object[] args)
		{
			SafeLog(LogLevel.Debug, message, exception, args);
		}

		public void Info(Func<string> message, params object[] args)
		{
			SafeLog(LogLevel.Information, message, args);
		}

		public void Info(Func<string> message, Exception exception, params object[] args)
		{
			SafeLog(LogLevel.Information, message, exception, args);
		}

		public void Warn(Func<string> message, params object[] args)
		{
			SafeLog(LogLevel.Warning, message, args);
		}

		public void Warn(Func<string> message, Exception exception, params object[] args)
		{
			SafeLog(LogLevel.Warning, message, exception, args);
		}

		public void Error(Func<string> message, params object[] args)
		{
			SafeLog(LogLevel.Error, message, args);
		}

		public void Error(Func<string> message, Exception exception, params object[] args)
		{
			SafeLog(LogLevel.Error, message, exception, args);
		}

		public void Critical(Func<string> message, params object[] args)
		{
			SafeLog(LogLevel.Critical, message, args);
		}

		public void Critical(Func<string> message, Exception exception, params object[] args)
		{
			SafeLog(LogLevel.Critical, message, exception, args);
		}

		private void SafeLog(LogLevel logLevel, Func<string> message, Exception exception, params object[] args)
		{
			if (!_inner.IsEnabled(logLevel))
				return;

			Interlocked.Exchange(ref _safe, 1);
			this.Log(logLevel, exception, message(), args);
		}

		private void SafeLog(LogLevel logLevel, Func<string> message, params object[] args)
		{
			if (!_inner.IsEnabled(logLevel))
				return;

			Interlocked.Exchange(ref _safe, 1);
			this.Log(logLevel, message(), args);
		}

		#region ILogger

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
			Func<TState, Exception, string> formatter)
		{
			if (_safe != 1)
				throw new InvalidOperationException("Logging operation was called in an unsafe way.");

			foreach (var interceptor in _interceptors)
			{
				if (!interceptor.CanIntercept ||
				    !interceptor.TryLog(_inner, ref _safe, logLevel, eventId, state, exception, formatter))
					continue;
				return;
			}

			_inner.Log(logLevel, eventId, state, exception, formatter);
			Interlocked.Exchange(ref _safe, 0);
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