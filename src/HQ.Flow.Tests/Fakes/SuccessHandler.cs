// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Threading.Tasks;

namespace HQ.Flow.Tests.Fakes
{
	public class SuccessHandler : IConsume<IEvent>
	{
		public int Handled { get; private set; }

		public Task<bool> HandleAsync(IEvent message)
		{
			Handled++;
			return Task.FromResult(true);
		}
	}
}