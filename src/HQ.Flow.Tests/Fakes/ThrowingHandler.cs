// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Threading.Tasks;

namespace HQ.Flow.Tests.Fakes
{
	public class ThrowingHandler : IConsume<IEvent>
	{
		public int Handled { get; private set; }

		public Task<bool> HandleAsync(IEvent message)
		{
			Handled++;
			throw new Exception();
		}
	}
}