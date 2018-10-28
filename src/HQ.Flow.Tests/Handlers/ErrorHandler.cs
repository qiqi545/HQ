using System;
using HQ.Flow.Bus;
using HQ.Flow.Tests.Messages;

namespace HQ.Flow.Tests.Handlers
{
	public class ErrorHandler : IMessageHandler<ErrorMessage>
	{
		public int Handled { get; private set; }

		public bool Handle(ErrorMessage message)
		{
			if(message.Error)
				throw new Exception("The message made me do it!");
			Handled++;
			return true;
		}
	}
}