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
using System.Threading.Tasks;
using HQ.Platform.Api.Notifications.Email.Providers;

namespace HQ.Platform.Api.Notifications.Email.Models
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

		public Task<bool> SendAsync(EmailMessage message)
		{
			var smtpMessage = SmtpEmailProvider.BuildMessageAndViews(message, out var textView, out var htmlView);
			try
			{
				_client().Send(smtpMessage);
				return Task.FromResult(true);
			}
			catch
			{
				return Task.FromResult(false);
			}
			finally
			{
				htmlView?.Dispose();
				textView?.Dispose();
			}
		}

		public async Task<IEnumerable<bool>> SendAsync(IEnumerable<EmailMessage> messages)
		{
			var result = new List<bool>();
			foreach (var message in messages)
				result.Add(await SendAsync(message));
			return result;
		}
	}
}