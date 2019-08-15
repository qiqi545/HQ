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

using HQ.Extensions.Scheduling.Models;

namespace HQ.Extensions.Notifications.Configuration
{
	public class NotificationDeliveryOptions
	{
		/// <summary>
		///     The location to store emails that are in queue when the delivery service is stopped, or based on retry policy
		/// </summary>
		public string BacklogFolder { get; set; }

		/// <summary>
		///     The location to store emails that were not successfully delivered, based on retry policy
		/// </summary>
		public string UndeliverableFolder { get; set; }

		#region PushQueue Configuration

		/// <summary>
		///     The retry policy for failed delivery attempts.
		/// </summary>
		public RetryPolicy RetryPolicy { get; set; }

		/// <summary>
		///     The rate limiting policy for email delivery.
		/// </summary>
		public RateLimitPolicy RateLimitPolicy { get; set; }

		/// <summary>
		///     The number of threads allowed to deliver email in parallel; the default is 10
		/// </summary>
		public int? MaxDegreeOfParallelism { get; set; }

		#endregion
	}
}