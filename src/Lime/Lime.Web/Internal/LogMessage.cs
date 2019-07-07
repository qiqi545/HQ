// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Logging;

namespace Lime.Web.Internal
{
	public class LogMessage<TState>
	{
		public LogMessage(LogLevel level, EventId eventId, TState state, Exception exception,
			Func<TState, Exception, string> formatter)
		{
			Level = level;
			EventId = eventId;
			Exception = exception;
			Message = formatter(state, exception);
		}

		public LogLevel Level { get; set; }
		public TState State { get; set; }
		public EventId EventId { get; set; }
		public Exception Exception { get; set; }
		public string Message { get; set; }
	}
}