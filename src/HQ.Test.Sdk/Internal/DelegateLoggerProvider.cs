using System;
using Microsoft.Extensions.Logging;

namespace HQ.Test.Sdk.Internal
{
	public sealed class DelegateLoggerProvider : ILoggerProvider
	{
		private readonly Action<string> _writeLine;

		public DelegateLoggerProvider(Action<string> writeLine)
		{
			_writeLine = writeLine;
		}

		public ILogger CreateLogger(string categoryName)
			=> new DelegateLogger(categoryName, _writeLine);

		public void Dispose()
		{
		}
	}
}
