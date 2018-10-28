using HQ.Flow.Bus;
using HQ.Flow.Tests.Messages;

namespace HQ.Flow.Tests.Handlers
{
	public class FailingHandler : IMessageHandler<IMessage>
	{
		public int Handled { get; private set; }

		public bool Handle(IMessage message)
		{
			Handled++;
			return false;
		}
	}
}
