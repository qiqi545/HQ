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
using HQ.Common;
using HQ.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace HQ.Platform.Api.Correlation
{
	public class TraceParentSafeLoggerInterceptor : ISafeLoggerInterceptor
	{
		private readonly IHttpContextAccessor _accessor;

		public TraceParentSafeLoggerInterceptor(IHttpContextAccessor accessor) => _accessor = accessor;

		public bool CanIntercept => _accessor?.HttpContext?.Request?.Headers != null &&
		                            _accessor.HttpContext.Request.Headers.TryGetValue(HeaderNames.TraceParent,
			                            out _);

		public bool TryLog<TState>(ILogger inner, ref int safe, LogLevel logLevel, EventId eventId, TState state,
			Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!_accessor.HttpContext.Request.Headers.TryGetValue(HeaderNames.TraceParent,
				out var traceContext))
				return false;

			try
			{
				// See: https://messagetemplates.org/
				var data = new Dictionary<string, object> {{$"@{HeaderNames.TraceParent}", traceContext}};

				using (inner.BeginScope(data))
				{
					inner.Log(logLevel, eventId, state, exception, formatter);
					Interlocked.Exchange(ref safe, 0);
					return true;
				}
			}
			catch (Exception ex) when (LogError(ex))
			{
				throw;
			}

			bool LogError(Exception ex)
			{
				inner.LogError(ex, "Unhandled exception");
				return true;
			}
		}
	}
}