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

using System;
using System.Runtime.Serialization;

namespace HQ.Extensions.Caching.Internal
{
	/// <summary>
	///     This exception indicates that a user of the TimedLock struct
	///     failed to leave a Monitor.  This could be the result of a
	///     deadlock or forgetting to use the using statement or a try
	///     finally block.
	/// </summary>
	[Serializable]
	public class UndisposedLockException : Exception
	{
		/// <summary>
		///     Constructor.
		/// </summary>
		/// <param name="message"></param>
		public UndisposedLockException(string message) : base(message)
		{
		}

		/// <summary>
		///     Special constructor used for deserialization.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected UndisposedLockException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}