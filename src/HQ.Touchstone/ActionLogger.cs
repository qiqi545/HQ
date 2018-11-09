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
using HQ.Common.Helpers;
using ImpromptuInterface;
using Microsoft.Extensions.Logging;

namespace HQ.Touchstone
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
            _formatter = DefaultFormatter;
            _categoryName = categoryName;
        }

        private static string DefaultFormatter(object[] args)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                var categoryName = args[0];
                var eventId = args[1];
                var message = args[2];

                sb.Append(categoryName?.ToString()?.ToLowerInvariant()).Append(':').Append(message).Append('[')
                    .Append(eventId).AppendLine("]");
            });
        }

        public IDisposable BeginScope<TState>(TState state) => this.ActLike<IDisposable>();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            _writeLine?.Invoke(_formatter?.Invoke(new object[] {_categoryName, eventId, formatter(state, exception)}));

            if (exception != null)
                _writeLine?.Invoke(exception.ToString());
        }

        public void Dispose() { }
    }
}
