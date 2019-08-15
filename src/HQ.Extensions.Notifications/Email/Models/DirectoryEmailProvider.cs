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
using System.Net;
using System.Net.Mail;

namespace HQ.Extensions.Notifications.Email.Models
{
	/// <summary>
	///     Delivers email as .EML files to a specified directory.
	/// </summary>
	public class DirectoryEmailProvider : IEmailProvider
	{
		private readonly Func<SmtpClient> _client;

		public DirectoryEmailProvider(string directory) =>
			_client = () => new SmtpClient
			{
				Host = "localhost",
				Credentials = CredentialCache.DefaultNetworkCredentials,
				DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
				PickupDirectoryLocation = directory
			};

		public bool Send(EmailMessage message)
		{
			AlternateView textView;
			AlternateView htmlView;
			var smtpMessage = SmtpEmailProvider.BuildMessageAndViews(message, out textView, out htmlView);
			try
			{
				_client().Send(smtpMessage);
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				if (htmlView != null) htmlView.Dispose();
				if (textView != null) textView.Dispose();
			}
		}

		public bool[] Send(IEnumerable<EmailMessage> messages)
		{
			var result = new List<bool>();
			foreach (var message in messages) Send(message);
			return result.ToArray();
		}
	}
}