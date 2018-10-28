using HQ.Flow.Bus;
using HQ.Flow.Tests.Messages;

namespace HQ.Flow.Tests.Handlers
{
	public class ManifoldMessageHandler : IMessageHandler<StringMessage>, IMessageHandler<IntegerMessage>
	{
		public int HandledString { get; private set; }
		public int HandledInteger { get; private set; }

		public bool Handle(StringMessage message)
		{
			HandledString++;
			return true;
		}

		public bool Handle(IntegerMessage message)
		{
			HandledInteger++;
			return true;
		}
	}
}