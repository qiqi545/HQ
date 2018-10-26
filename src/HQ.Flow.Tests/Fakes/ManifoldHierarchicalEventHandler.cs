// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Threading.Tasks;

namespace HQ.Flow.Tests.Fakes
{
	public class ManifoldHierarchicalEventHandler : IConsume<BaseEvent>, IConsume<InheritedEvent>, IConsume<ErrorEvent>,
		IConsume<IEvent>
	{
		public int HandledInterface { get; set; }
		public int HandledBase { get; set; }
		public int HandledInherited { get; set; }

		public Task<bool> HandleAsync(BaseEvent message)
		{
			HandledBase++;
			return Task.FromResult(true);
		}

		public Task<bool> HandleAsync(ErrorEvent message)
		{
			throw new Exception("the message made me do it!");
		}

		public Task<bool> HandleAsync(IEvent message)
		{
			HandledInterface++;
			return Task.FromResult(true);
		}

		public Task<bool> HandleAsync(InheritedEvent message)
		{
			HandledInherited++;
			return Task.FromResult(true);
		}
	}
}