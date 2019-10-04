﻿#region LICENSE

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
using System.IO;
using HQ.Extensions.Notifications.Configuration;
using HQ.Extensions.Notifications.Email.Models;
using HQ.Extensions.Notifications.Email.Providers;
using HQ.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Extensions.Notifications
{
	public static class Add
	{
		public static IServiceCollection AddEmailNotifications(this IServiceCollection services, IConfiguration config)
		{
			return services.AddEmailNotifications(config.FastBind);
		}

		public static IServiceCollection AddEmailNotifications(this IServiceCollection services,
			Action<EmailOptions> configureAction = null)
		{
			if (configureAction != null)
				services.Configure(configureAction);

			var options = new EmailOptions();
			configureAction?.Invoke(options);

			switch (options.Provider)
			{
				case nameof(DirectoryEmailProvider):
					services.AddSingleton<IEmailProvider>(r =>
					{
						Directory.CreateDirectory(options.ProviderKey);
						return new DirectoryEmailProvider(options.ProviderKey);
					});
					break;

				case nameof(MemoryEmailProvider):
					services.AddSingleton<IEmailProvider>(r => new MemoryEmailProvider());
					break;

				case nameof(PostmarkEmailProvider):
					services.AddSingleton<IEmailProvider>(r => new PostmarkEmailProvider(options.ProviderKey));
					break;

				default:
					throw new NotSupportedException($"No email provider named '{options.Provider}' is available.");
			}

			services.AddSingleton<IEmailService, EmailService>();

			return services;
		}
	}
}