#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System.Collections.Generic;

namespace HQ.Extensions.Metrics.Stats
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