// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Blowdart.UI.Web.Internal
{
    internal class ServerLogger : ILogger
    {
        private readonly IHubContext<LoggingHub> _context;

        public ServerLogger(IHubContext<LoggingHub> context)
        {
            _context = context;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;
            _context.Clients.All.SendAsync(MessageTypes.Log, new LogMessage<TState>(logLevel, eventId, state, exception, formatter));
        }
    }
}