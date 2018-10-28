using System;
using HQ.Flow.Bus;
using HQ.Flow.Tests.Messages;

namespace HQ.Flow.Tests.Handlers
{
	public class ManifoldHierarchicalMessageHandler :
		IMessageHandler<BaseMessage>,
		IMessageHandler<InheritedMessage>,
		IMessageHandler<ErrorMessage>,
		IMessageHandler<IMessage>
	{
		public int HandledInterface { get; set; }
		public int HandledBase { get; set; }
		public int HandledInherited { get; set; }

		public bool Handle(BaseMessage message)
		{
			HandledBase++;
			return true;
		}

		public bool Handle(InheritedMessage message)
		{
			HandledInherited++;
			return true;
		}

		public bool Handle(IMessage message)
		{
			HandledInterface++;
			return true;
		}

		public bool Handle(ErrorMessage message)
		{
			if(message.Error)
				throw new Exception("the message made me do it!");
			return true;
		}
	}
}