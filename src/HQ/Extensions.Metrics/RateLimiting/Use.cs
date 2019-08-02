using System;
using HQ.Extensions.Metrics.RateLimiting.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace HQ.Extensions.Metrics.RateLimiting
{
    public static class Use
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app, IConfiguration configuration)
        {
            return app.UseRateLimiting(configuration.Bind);
        }

        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app, Action<RateLimitingOptions> configureAction = null)
        {
            return app;
        }
    }
}
