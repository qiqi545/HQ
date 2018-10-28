// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

namespace HQ.Flow.Tests.Bus.Messages
{
	public class BaseMessage : IMessage
	{
		public int Id { get; set; }
	}
}