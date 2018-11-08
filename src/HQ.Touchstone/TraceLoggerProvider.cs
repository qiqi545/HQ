using System;
using Microsoft.Extensions.Logging;

namespace HQ.Touchstone
{
	internal sealed class ActionLoggerProvider : ILoggerProvider
	{
		private readonly Action<string> _writeLine;

		public ActionLoggerProvider(Action<string> writeLine)
		{
			_writeLine = writeLine;
		}

		public ILogger CreateLogger(string categoryName)
			=> new ActionLogger(categoryName, _writeLine);

		public void Dispose() { }
	}
}