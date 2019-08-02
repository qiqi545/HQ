using System;
using HQ.Common;

namespace HQ.Platform.Security.Configuration
{
	/// <summary>
	/// See: https://hstspreload.org
	/// </summary>
	public class HstsOptions : FeatureToggle
	{
		public HstsPreloadStage Stage { get; set; } = HstsPreloadStage.One;
		public bool IncludeSubdomains { get; set; } = true;
		public bool Preload { get; set; } = false;

		public TimeSpan HstsMaxAge
		{
			get
			{
				var now = DateTimeOffset.UtcNow;

				switch (Stage)
				{
					case HstsPreloadStage.One:
						return TimeSpan.FromMinutes(5);
					case HstsPreloadStage.Two:
						return TimeSpan.FromDays(7);
					case HstsPreloadStage.Three:
						return now.AddMonths(1) - now;
					case HstsPreloadStage.ReadyForPreload:
						return now.AddYears(2) - now;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}