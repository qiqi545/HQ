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
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Identity.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static bool TryGetTenantId<TKey>(this IServiceProvider services, out TKey tenantId)
        {
            var security = services?.GetService(typeof(IOptions<SecurityOptions>)) as IOptions<SecurityOptions>;
            if (security?.Value?.Claims != null)
                return services.TryGetClaim(security.Value.Claims.TenantIdClaim, out tenantId);

            tenantId = default;
            return false;
        }

        public static bool TryGetTenantName(this IServiceProvider services, out string tenantName)
        {
            var security = services?.GetService(typeof(IOptions<SecurityOptions>)) as IOptions<SecurityOptions>;
            if (security?.Value?.Claims != null)
                return services.TryGetClaim(security.Value.Claims.TenantNameClaim, out tenantName);

            tenantName = default;
            return false;
        }

        public static bool TryGetClaim<TKey>(this IServiceProvider services, string type, out TKey value)
        {
            var accessor = services?.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            var user = accessor?.HttpContext?.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                value = default;
                return false;
            }

            var claim = user.FindFirst(type);
            if (claim == null)
            {
                value = default;
                return false;
            }

            value = (TKey)Convert.ChangeType(claim.Value, typeof(TKey));
            return true;
        }
    }
}
