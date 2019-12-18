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
using HQ.Common.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace HQ.Common.AspNetCore
{
	public static class HttpContextExtensions
	{
		public static async Task WriteResultAsJson(this IApplicationBuilder app, HttpContext context, string json,
			CancellationToken? cancellationToken = null)
		{
			context.Response.Headers.Add(HeaderNames.ContentType, MediaTypeNames.Application.Json);
			await context.Response.WriteAsync(json, cancellationToken ?? context.RequestAborted);
		}

		public static async Task WriteResultAsJson(this IApplicationBuilder app, HttpContext context, object instance,
			CancellationToken? cancellationToken = null)
		{
			context.Response.Headers.Add(HeaderNames.ContentType, MediaTypeNames.Application.Json);
			await context.Response.WriteAsync(SerializeObject(app, context, instance),
				cancellationToken ?? context.RequestAborted);
		}

		public static string SerializeObject(this IApplicationBuilder app, HttpContext context, object instance)
		{
			var serializerSettings = app.ApplicationServices.GetService<JsonSerializerSettings>();
			if (serializerSettings != null)
			{
				if (context.Items[Constants.ContextKeys.JsonMultiCase] is ITextTransform transform)
				{
					serializerSettings.ContractResolver =
						new JsonContractResolver(transform, JsonProcessingDirection.Output);
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
	}
}