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

using HQ.Common;
using HQ.Platform.Api.Models;
using Microsoft.AspNetCore.Http;

namespace HQ.Platform.Api.Extensions
{
    public static class HttpContextExtensions
    {
        public static void SetTenantContext<TTenant>(this HttpContext context, TenantContext<TTenant> tenantContext)
            where TTenant : class
        {
            context.Items[Constants.ContextKeys.Tenant] = tenantContext;
        }

        public static TenantContext<TTenant> GetTenantContext<TTenant>(this HttpContext context) where TTenant : class
        {
            return context.Items.TryGetValue(Constants.ContextKeys.Tenant, out var tenantContext)
                ? tenantContext as TenantContext<TTenant>
                : default;
        }
    }
}
