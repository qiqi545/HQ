// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Lime.Web.Internal
{
	internal class ServerLogger : ILogger
	{
		private readonly IHubContext<LoggingHub> _context;

		public ServerLogger(IHubContext<LoggingHub> context) => _context = context;

		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
			Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
				return;
			var message = new LogMessage<TState>(logLevel, eventId, state, exception, formatter);
			_context.Clients.All.SendAsync(MessageTypes.Log, message);
		}
	}
}