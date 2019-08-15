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
using System.Collections.Generic;
using HQ.Extensions.Notifications.Models;

namespace HQ.Extensions.Notifications.Email.Models
{
	public class EmailMessage
	{
		public EmailMessage()
		{
			Id = Guid.NewGuid();
			To = new List<string>(0);
			ReplyTo = new List<string>(0);
			Cc = new List<string>(0);
			Bcc = new List<string>(0);
			Headers = new List<NameValuePair>(0);
		}

		public Guid Id { get; set; }
		public List<string> To { get; set; }
		public List<string> ReplyTo { get; set; }
		public string From { get; set; }
		public List<string> Cc { get; set; }
		public List<string> Bcc { get; set; }
		public string Subject { get; set; }
		public string TextBody { get; set; }
		public string HtmlBody { get; set; }
		public List<NameValuePair> Headers { get; set; }

		public bool Delivered { get; set; }
		public int DeliveryAttempts { get; set; }
		public DateTime? DeliveryTime { get; internal set; }
		public DateTime? DeliveredAt { get; set; }

		public void AddHeader(string name, string value)
		{
			Headers.Add(new NameValuePair {Name = name, Value = value});
		}
	}
}