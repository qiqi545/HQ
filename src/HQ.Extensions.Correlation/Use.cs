using HQ.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace HQ.Extensions.Correlation
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
                    context.Request.Headers.Add(Constants.HttpHeaders.TraceParent, traceContext = TraceContext.New().Header);

                context.TraceIdentifier = traceContext;

                if (app.ApplicationServices.GetService(typeof(IHttpContextAccessor)) is IHttpContextAccessor accessor)
                    accessor.HttpContext = context;

                await next();
            });

            return app;
        }
    }
}
