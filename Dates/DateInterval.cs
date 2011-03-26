using System;

namespace Dates
{
    /// <summary>
    /// Used when calculating the difference between two <see cref="DateTime" /> instances
    /// with the <see cref="DateSpan" /> class.
    /// </summary>
    public enum DateInterval
    {
        Years,
        Months,
        Weeks,
        Days,
        Hours,
        Minutes,
        Seconds
    }
}