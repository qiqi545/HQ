using System;

namespace HQ.Common.Models
{
	public interface IServerTimestampService
	{
		DateTimeOffset GetCurrentTime();
	}
}
