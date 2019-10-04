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
using ImpromptuInterface;
using Microsoft.Extensions.Logging;
using TypeKitchen;

namespace HQ.Extensions.Logging
{
	internal sealed class ActionLogger : ILogger
	{
		private readonly string _categoryName;
		private readonly Func<object[], string> _formatter;
		private readonly Action<string> _writeLine;

		public ActionLogger(string categoryName,
			Action<string> writeLine,
			Func<object[], string> formatter = null)
		{
			_writeLine = writeLine;
			_formatter = formatter ?? DefaultFormatter;
			_categoryName = categoryName;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return this.ActLike<IDisposable>();
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
			Func<TState, Exception, string> formatter)
		{
			_writeLine?.Invoke(_formatter?.Invoke(new object[]
			{
				logLevel, _categoryName, eventId, formatter(state, exception)
			}));

			if (exception != null)
				_writeLine?.Invoke(exception.ToString());
		}

		private static string DefaultFormatter(object[] args)
		{
			return Pooling.StringBuilderPool.Scoped(sb =>
			{
				var logLevelString = GetLogLevelShorthand(args);

				var categoryName = args[1];
				var eventId = args[2];
				var message = args[3];

				sb.Append(logLevelString).Append(':')
					.Append(categoryName).Append('[').Append(eventId).AppendLine("]")
					.Append('\t').Append(message);
			});
		}

		private static string GetLogLevelShorthand(IReadOnlyList<object> args)
		{
			var logLevel = (LogLevel) args[0];
			string logLevelString;
			switch (logLevel)
			{
				case LogLevel.Trace:
					logLevelString = "trace";
					break;
				case LogLevel.Debug:
					logLevelString = "dbug";
					break;
				case LogLevel.Information:
					logLevelString = "info";
					break;
				case LogLevel.Warning:
					logLevelString = "warn";
					break;
				case LogLevel.Error:
					logLevelString = "error";
					break;
				case LogLevel.Critical:
					logLevelString = "crit";
					break;
				case LogLevel.None:
					logLevelString = "";
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return logLevelString;
		}

		public void Dispose()
		{
		}
	}
}