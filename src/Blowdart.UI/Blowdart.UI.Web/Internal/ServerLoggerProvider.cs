// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Blowdart.UI.Web.Internal
{
    internal class ServerLoggerProvider : ILoggerProvider
    {
        private readonly IHubContext<LoggingHub> _context;

        public ServerLoggerProvider(IHubContext<LoggingHub> context)
        {
            _context = context;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new ServerLogger(_context);
        }

        public void Dispose() { }
    }
}