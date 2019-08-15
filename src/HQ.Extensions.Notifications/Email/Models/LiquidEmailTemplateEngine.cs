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
using DotLiquid;
using HQ.Extensions.Notifications.Extensions;
using HQ.Extensions.Notifications.Models;
using WyHash;

namespace HQ.Extensions.Notifications.Email.Models
{
	internal class LiquidEmailTemplateEngine : IEmailTemplateEngine
	{
		private static readonly IDictionary<ulong, Template> Compiled = new Dictionary<ulong, Template>();

		public EmailMessage CreateTextEmail(string textTemplate, dynamic model)
		{
			var hash = HashExtensions.FromDynamic(model);
			var textBody = PrepareBodyFromTemplate(textTemplate, hash);
			var wrapped = WrapEmail(hash);
			var email = new EmailMessage
			{
				From = wrapped.From, To = wrapped.To, Subject = wrapped.Subject, TextBody = textBody
			};

			return email;
		}

		public EmailMessage CreateCombinedEmail(string htmlTemplate, string textTemplate, dynamic model)
		{
			var hash = HashExtensions.FromDynamic(model);
			var htmlBody = PrepareBodyFromTemplate(htmlTemplate, hash);
			var textBody = PrepareBodyFromTemplate(textTemplate, hash);

			var wrapped = WrapEmail(hash);
			var email = new EmailMessage
			{
				From = wrapped.From,
				To = wrapped.To,
				Subject = wrapped.Subject,
				TextBody = textBody,
				HtmlBody = htmlBody
			};

			return email;
		}

		public EmailMessage CreateHtmlEmail(string htmlTemplate, dynamic model)
		{
			var hash = HashExtensions.FromDynamic(model);
			var htmlBody = PrepareBodyFromTemplate(htmlTemplate, hash);

			var wrapped = WrapEmail(hash);
			var email = new EmailMessage
			{
				From = wrapped.From, To = wrapped.To, Subject = wrapped.Subject, HtmlBody = htmlBody
			};

			return email;
		}


		private static dynamic WrapEmail(dynamic hash)
		{
			dynamic wrapped = new SafeHash((Hash) hash);
			if (!(wrapped.To is List<string>)) wrapped.To = new List<string>(new[] {wrapped.To as string});
			return wrapped;
		}

		private static string PrepareBodyFromTemplate(string template, dynamic hash)
		{
			string htmlBody = null;
			if (!string.IsNullOrWhiteSpace(template))
			{
				var cacheKey = WyHash64.ComputeHash64(template);
				Template htmlTemplate;
				if (Compiled.ContainsKey(cacheKey))
					htmlTemplate = Compiled[cacheKey];
				else
				{
					htmlTemplate = Template.Parse(template);
					Compiled.Add(cacheKey, htmlTemplate);
				}

				htmlBody = htmlTemplate.Render(hash);
			}

			return htmlBody;
		}
	}
}