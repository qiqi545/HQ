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
using System.Threading;
using HQ.Tokens.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HQ.Cohort.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static bool TryGetRequestAbortCancellationToken(this IServiceProvider services,
            out CancellationToken cancelToken)
        {
            cancelToken = CancellationToken.None;
            var accessor = services?.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            var token = accessor?.HttpContext?.RequestAborted;
            if (!token.HasValue)
                return false;
            cancelToken = token.Value;
            return true;
        }

        public static bool TryGetTenantId(this IServiceProvider services, out int tenantId)
        {
            var accessor = services?.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            var user = accessor?.HttpContext?.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                tenantId = 0;
                return false;
            }

            var security = services?.GetService(typeof(IOptions<SecurityOptions>)) as IOptions<SecurityOptions>;
            if(security?.Value?.Claims == null)
            {
                tenantId = 0;
                return false;
            }

            var claim = user.FindFirst(security.Value.Claims.TenantIdClaim);
            if(claim == null)
            {
                tenantId = 0;
                return false;
            }

            return int.TryParse(claim.Value, out tenantId);
        }
    }
}
