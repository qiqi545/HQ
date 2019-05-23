using System;

namespace HQ.Extensions.Caching.Configuration
{
    public class CacheOptions
    {
        public TimeSpan? ContentionTimeout { get; set; }
        public long? MaxSizeBytes { get; set; }
    }
}
