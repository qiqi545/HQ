using HQ.Flow.Bus;
using HQ.Flow.Tests.Messages;

namespace HQ.Flow.Tests.Handlers
{
	public class StringMessageHandler2 : IMessageHandler<StringMessage>
	{
		public int Handled { get; private set; }

		public bool Handle(StringMessage message)
		{
			Handled++;
			return true;
		}
	}
}