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
using Newtonsoft.Json;

namespace HQ.Common
{
	public interface IServerTimestampService
	{
		/// <summary> Retrieves the current instantaneous time, preserving time zone.</summary>
		DateTimeZone GetCurrentZonedTime();

		/// <summary>
		///     Retrieves the current instantaneous time, with time zone offset. This should only be used for operator-level
		///     activities that are not displayed to a user, such as transaction logs.
		/// </summary>
		DateTimeOffset GetCurrentTime();

		/// <summary> Retrieves the current instantaneous tick time. This is only suitable for use as an index.</summary>
		long GetCurrentTimestamp();

		/// <summary>
		///     Retrieves <see cref="JsonSerializerSettings" /> suitable for communicating time across systems. Note that
		///     time zone information is not included on the wire without passing it explicitly in another field.
		/// </summary>
		JsonSerializerSettings GetDateTimeSerializerSettings();
	}
}