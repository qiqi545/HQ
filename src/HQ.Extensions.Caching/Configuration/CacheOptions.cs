using System;
using System.Collections.Generic;
using System.Text;

namespace HQ.Extensions.Caching.Configuration
{
    public class CacheOptions
    {
        public TimeSpan? ContentionTimeout { get; set; }
    }
}
