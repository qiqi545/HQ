using System;
using ImpromptuInterface;
using Microsoft.Extensions.Logging;

namespace HQ.Touchstone
{
	internal sealed class ActionLogger : ILogger
	{
		private readonly Action<string> _writeLine;
	    private readonly Func<object[], string> _formatter;
	    private readonly string _categoryName;

		public ActionLogger(string categoryName,
		    Action<string> writeLine,
		    Func<object[], string> formatter = null)
		{
            _writeLine = writeLine;
		    _formatter = args => string.Join(" ", args);
		    _categoryName = categoryName;
		}

		public IDisposable BeginScope<TState>(TState state) => this.ActLike<IDisposable>();

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
		    _writeLine?.Invoke(_formatter?.Invoke(new object[] {_categoryName, eventId, formatter(state, exception)}));

			if (exception != null)
				_writeLine?.Invoke(exception.ToString());
		}

		public void Dispose() { }
	}
}
