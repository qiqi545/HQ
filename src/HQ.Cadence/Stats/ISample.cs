// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;

namespace HQ.Cadence.Stats
{
	/// <summary>
	///     A statistically representative sample of a data stream
	/// </summary>
	public interface ISample
	{
		/// <summary>
		///     Returns the number of values recorded
		/// </summary>
		int Count { get; }

		/// <summary>
		///     Returns a copy of the sample's values
		/// </summary>
		ICollection<long> Values { get; }

		/// <summary>
		///     Clears all recorded values
		/// </summary>
		void Clear();

		/// <summary>
		///     Adds a new recorded value to the sample
		/// </summary>
		void Update(long value);
	}
}