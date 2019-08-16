using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HQ.Extensions.Notifications.Email.Models;
using PostmarkDotNet;
using PostmarkDotNet.Model;

namespace HQ.Extensions.Notifications.Email.Providers
{
	public class PostmarkEmailProvider : IEmailProvider
	{
		private const int BatchMax = 500;

		private readonly PostmarkClient _client;

		public PostmarkEmailProvider(string serverToken)
		{
			_client = new PostmarkClient(serverToken);
		}

		public async Task<bool> SendAsync(EmailMessage message)
		{
			message.DeliveryAttempts++;
			var pm = CreatePostmarkMessage(message);
			var response = await _client.SendMessageAsync(pm);
			message.Delivered = response.Status == PostmarkStatus.Success;
			if (message.Delivered)
			{
				message.DeliveredAt = DateTime.UtcNow;
			}
			return message.Delivered;
		}

		private static PostmarkMessage CreatePostmarkMessage(EmailMessage message)
		{
			var pm = new PostmarkMessage
			{
				From = message.From,
				Subject = message.Subject,
				TextBody = message.TextBody,
				HtmlBody = message.HtmlBody,
				To = message.To.Count > 0 ? string.Join(",", message.To) : "",
				ReplyTo = message.ReplyTo.Count > 0 ? string.Join(",", message.ReplyTo) : "",
				Cc = message.Cc.Count > 0 ? string.Join(",", message.Cc) : "",
				Bcc = message.Bcc.Count > 0 ? string.Join(",", message.Bcc) : ""
			};
			foreach (var header in message.Headers)
			{
				pm.Headers.Add(new MailHeader(header.Name, header.Value));
			}
			return pm;
		}

		public async Task<IEnumerable<bool>> SendAsync(IEnumerable<EmailMessage> messages)
		{
			// Postmark only allows 500 messages per batch, so we'll send successive batches
			var payload = messages.ToList();
			var results = new List<bool>();
			if (payload.Count <= BatchMax)
				return results.ToArray();
			var batches = (int)Math.Ceiling(payload.Count / (double)BatchMax);
			for (var i = 0; i < batches; i++)
			{
				var slice = payload.Skip(i * BatchMax).Take(BatchMax).ToList();
				results.Add(await SendBatch(slice));
			}
			return results;
		}

		private async Task<bool> SendBatch(IList<EmailMessage> payload)
		{
			var pm = payload.Select(CreatePostmarkMessage).ToList();
			var responses = (await _client.SendMessagesAsync(pm)).ToList();
			for (var i = 0; i < responses.Count; i++)
			{
				payload[i].DeliveryAttempts++;
				if (responses[i].Status != PostmarkStatus.Success)
				{
					continue;
				}
				payload[i].Delivered = true;
				payload[i].DeliveredAt = DateTime.UtcNow;
			}
			return responses.All(r => r.Status == PostmarkStatus.Success);
		}
	}
}
