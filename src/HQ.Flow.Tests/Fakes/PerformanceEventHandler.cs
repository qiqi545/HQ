// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Threading.Tasks;

namespace HQ.Flow.Tests.Fakes
{
	public class PerformanceEventHandler : IConsume<StringEvent>, IConsume<IntegerEvent>
	{
		public int HandledString { get; private set; }
		public int HandledInteger { get; private set; }

		public Task<bool> HandleAsync(IntegerEvent message)
		{
			HandledInteger++;
			return Task.FromResult(true);
		}

		public Task<bool> HandleAsync(StringEvent message)
		{
			HandledString++;
			return Task.FromResult(true);
		}
	}
}