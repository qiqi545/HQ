using HQ.Flow.Bus;

namespace HQ.Flow.Tests.Messages
{
	public class IncrementingHandler : IMessageHandler<IMessage>
	{
		public int Handled { get; private set; }

		public bool Handle(IMessage message)
		{
			Handled++;
			return true;
		}
	}
}