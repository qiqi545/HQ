using System;
using System.Diagnostics;

namespace HQ.Touchstone
{
	internal class ActionTraceListener : TraceListener
	{
		private readonly Action<string> _write;
		private readonly Action<string> _writeLine;

		public ActionTraceListener(Action<string> write, Action<string> writeLine = null)
		{
			_write = write;
			_writeLine = writeLine ?? write;
		}

		public override void WriteLine(string str)
		{
			_writeLine?.Invoke(str);
		}

		public override void Write(string str)
		{
			_write?.Invoke(str);
		}
	}
}
