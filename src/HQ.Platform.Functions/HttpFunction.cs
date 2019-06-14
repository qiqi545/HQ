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
using System.ComponentModel;
using System.Net.Http;
using HQ.Extensions.Scheduling.Hooks;
using HQ.Extensions.Scheduling.Models;

namespace HQ.Platform.Functions
{
    [Description("Invokes an HTTP request.")]
    public class HttpFunction : Before, Handler
    {
        private const string RequestUriKey = "RequestUri";

        public void Before(ExecutionContext context)
        {
            context.TryGetData(RequestUriKey, out string uriString);
            if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri))
            {
                context.Fail();
            }
            else
            {
                context.AddData(RequestUriKey, uri);
            }
        }

        public void Perform(ExecutionContext context)
        {
            context.TryGetData(RequestUriKey, out Uri requestUri);

            using (var handler = new HttpClientHandler())
            {
                using (var client = new HttpClient(handler))
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    var response = client.SendAsync(request, context.CancellationToken).ConfigureAwait(false)
                        .GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        context.Succeed();
                    }
                    else
                    {
                        context.Fail();
                    }
                }
            }
        }
    }
}
