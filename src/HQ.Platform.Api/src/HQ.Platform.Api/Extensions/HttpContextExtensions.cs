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

using System.Threading;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Platform.Api.Models;
using HQ.Platform.Api.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace HQ.Platform.Api.Extensions
{
    public static class HttpContextExtensions
    {
        public static async Task WriteResultAsJson(this IApplicationBuilder app, HttpContext context, object instance, CancellationToken? cancellationToken = null)
        {
            context.Response.Headers.Add(Constants.HttpHeaders.ContentType, Constants.MediaTypes.Json);
            await context.Response.WriteAsync(SerializeObject(app, context, instance), cancellationToken ?? context.RequestAborted);
        }

        public static string SerializeObject(this IApplicationBuilder app, HttpContext context, object instance)
        {
            var serializerSettings = app.ApplicationServices.GetService<JsonSerializerSettings>();
            if (serializerSettings != null)
            {
                if (context.Items[Constants.ContextKeys.JsonMultiCase] is ITextTransform transform)
                {
                    serializerSettings.ContractResolver = new JsonContractResolver(transform, JsonProcessingDirection.Output);
                }
                else
                {
                    serializerSettings = JsonConvert.DefaultSettings();
                }
            }

            var json = serializerSettings != null
                ? JsonConvert.SerializeObject(instance, serializerSettings)
                : JsonConvert.SerializeObject(instance);

            return json;
        }

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

        public static void SetApplicationContext<TApplication>(this HttpContext context, ApplicationContext<TApplication> tenantContext)
            where TApplication : class
        {
            context.Items[Constants.ContextKeys.Application] = tenantContext;
        }

        public static ApplicationContext<TApplication> GetApplicationContext<TApplication>(this HttpContext context) where TApplication : class
        {
            return context.Items.TryGetValue(Constants.ContextKeys.Application, out var tenantContext)
                ? tenantContext as ApplicationContext<TApplication>
                : default;
        }

        public static void SetVersionContext(this HttpContext context, VersionContext versionContext)
        {
            context.Items[Constants.ContextKeys.Version] = versionContext;
        }

        public static VersionContext GetVersionContext(this HttpContext context)
        {
            return context.Items.TryGetValue(Constants.ContextKeys.Version, out var versionContext)
                ? versionContext as VersionContext
                : default;
        }
    }
}
