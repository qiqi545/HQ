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
using HQ.Common.Configuration;

namespace HQ.Platform.Security.Configuration
{
    public enum HstsPreloadStage { One, Two, Three, ReadyForPreload }

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

    public class HttpsOptions : FeatureToggle
    {
        public HstsOptions Hsts { get; set; } = new HstsOptions();
    }
}
