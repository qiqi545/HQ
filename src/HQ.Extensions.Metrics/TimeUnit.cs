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

namespace HQ.Extensions.Metrics
{
	/// <summary>
	///     Provides support for timing values
	///     <see href="http://download.oracle.com/javase/6/docs/api/java/util/concurrent/TimeUnit.html" />
	/// </summary>
	public enum TimeUnit
	{
		Nanoseconds = 0,
		Microseconds = 1,
		Milliseconds = 2,
		Seconds = 3,
		Minutes = 4,
		Hours = 5,
		Days = 6
	}
}