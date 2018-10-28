using System;
using HQ.Flow.Bus;
using HQ.Flow.Tests.Messages;

namespace HQ.Flow.Tests.Handlers
{
	public class ThrowingHandler : IMessageHandler<IMessage>
	{
		public int Handled { get; private set; }

		public bool Handle(IMessage message)
		{
			Handled++;
			throw new Exception();
		}
	}
}