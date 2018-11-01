using System;

namespace HQ.Common.Models
{
	public class LocalServerTimestampService : IServerTimestampService
	{
		public DateTimeOffset GetCurrentTime()
		{
			return DateTimeOffset.Now;
		}
	}
}