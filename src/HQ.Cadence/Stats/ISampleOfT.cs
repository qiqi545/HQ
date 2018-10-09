// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

namespace HQ.Cadence.Stats
{
	/// <summary>
	///     A statistically representative sample of a data stream
	/// </summary>
	public interface ISample<out T> : ISample, ICopyable<T> { }
}