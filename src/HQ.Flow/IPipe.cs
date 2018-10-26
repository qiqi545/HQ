// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

namespace HQ.Flow
{
	/// <summary>
	///     A pipe produces on one end and consumers from another.
	/// </summary>
	/// <typeparam name="TP"></typeparam>
	/// <typeparam name="TC"></typeparam>
	public interface IPipe<out TP, in TC> : IProduce<TP>, IConsume<TC>
	{
	}
}