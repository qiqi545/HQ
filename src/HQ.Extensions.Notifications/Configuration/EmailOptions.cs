using HQ.Extensions.Notifications.Email.Models;

namespace HQ.Extensions.Notifications.Configuration
{
	public class EmailOptions
	{
		public string Provider { get; set; } = nameof(DirectoryEmailProvider);
		public string ProviderKey { get; set; } = "/email";
		public string From { get; set; }
	}
}
