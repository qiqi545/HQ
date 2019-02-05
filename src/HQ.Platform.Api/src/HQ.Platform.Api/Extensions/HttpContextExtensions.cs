using System;
using Microsoft.AspNetCore.Http;
using HQ.Common;
using HQ.Platform.Api.Models;

namespace HQ.Platform.Api.Extensions
{
    public static class HttpContextExtensions
    {
        public static void SetTenantContext<TTenant>(this HttpContext context, TenantContext<TTenant> tenantContext) where TTenant : class
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
