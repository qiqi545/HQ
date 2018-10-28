// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using HQ.Flow.Bus;
using HQ.Flow.Tests.Bus.Messages;

namespace HQ.Flow.Tests.Bus.Handlers
{
	public class ManifoldMessageHandler : IMessageHandler<StringMessage>, IMessageHandler<IntegerMessage>
	{
		public int HandledString { get; private set; }
		public int HandledInteger { get; private set; }

		public bool Handle(IntegerMessage message)
		{
			HandledInteger++;
			return true;
		}

		public bool Handle(StringMessage message)
		{
			HandledString++;
			return true;
		}
	}
}