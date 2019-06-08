using HQ.Common;
using HQ.Extensions.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace HQ.Extensions.Logging
{
    public static class Use
    {
        // 
        // See: https://www.w3.org/TR/trace-context/#problem-statement
        public static IApplicationBuilder UseTraceContext(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if (!context.Request.Headers.TryGetValue(Constants.HttpHeaders.TraceParent, out var traceContext))
                    context.Request.Headers.Add(Constants.HttpHeaders.TraceParent, traceContext = GenerateTraceContext());

                context.TraceIdentifier = traceContext;

                if (app.ApplicationServices.GetService(typeof(IHttpContextAccessor)) is IHttpContextAccessor accessor)
                    accessor.HttpContext = context;

                await next();
            });

            return app;
        }

        private static string GenerateTraceContext()
        {
            var traceId = Crypto.GetRandomString(16 * 2);//.ToLowerInvariant();
            var parentId = Crypto.GetRandomString(8 * 2).ToLowerInvariant();
            return $"00-{traceId}-{parentId}-00";
        }
    }
}
