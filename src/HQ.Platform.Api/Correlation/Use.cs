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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace HQ.Platform.Api.Correlation
{
	public static class Use
	{
		// 
		// See: https://www.w3.org/TR/trace-context/#problem-statement
		public static IApplicationBuilder UseTraceContext(this IApplicationBuilder app)
		{
			app.Use(async (context, next) =>
			{
				if (!context.Request.Headers.TryGetValue(HeaderNames.TraceParent, out var traceContext))
				{
					context.Request.Headers.Add(HeaderNames.TraceParent,
						traceContext = TraceContext.New().Header);
				}

				context.TraceIdentifier = traceContext;

				if (app.ApplicationServices.GetService(typeof(IHttpContextAccessor)) is IHttpContextAccessor accessor)
				{
					accessor.HttpContext = context;
				}

				await next();
			});

			return app;
		}
	}
}