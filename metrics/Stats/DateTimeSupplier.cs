using System;

namespace metrics.Stats
{
    internal class DateTimeSupplier : IDateTimeSupplier
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}