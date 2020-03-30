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
using System.Threading.Tasks;
using HQ.Platform.Api.Notifications.Configuration;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Api.Notifications.Email.Models
{
	internal sealed class EmailService : IEmailService
	{
		private readonly IOptionsSnapshot<EmailOptions> _options;
		private readonly IEmailProvider _provider;

		public EmailService(IEmailProvider provider, IOptionsSnapshot<EmailOptions> options)
		{
			_provider = provider;
			_options = options;
		}

		public async Task<bool> SendAsync(EmailMessage message)
		{
			message.From ??= _options.Value.From;

			return await _provider.SendAsync(message);
		}

		public async Task<IEnumerable<bool>> SendAsync(IEnumerable<EmailMessage> messages)
		{
			return await _provider.SendAsync(messages);
		}
	}
}