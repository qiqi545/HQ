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

using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using HQ.Data.Contracts.Runtime;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace HQ.Platform.Runtime.Rest.Models
{
    public class RestMutationContextProvider : IMutationContextProvider
    {
        public IEnumerable<MutationContext> Parse(HttpContext source)
        {
            var context = new MutationContext(source.User);

            // context.Type = ...

            BuildHandleData(source.Request, context);

            // context.Handle = ...

            yield return context;
        }

        public IEnumerable<MutationContext> Parse(ClaimsPrincipal user, string source)
        {
            var context = new MutationContext(user);

            BuildHandleData(context, source);

            yield return context;
        }

        private static void BuildHandleData(HttpRequest source, MutationContext context)
        {
            using (var sr = new StreamReader(source.Body))
            {
                var json = sr.ReadToEnd();

                BuildHandleData(context, json);
            }
        }

        private static void BuildHandleData(MutationContext context, string source)
        {
            var body = JsonConvert.DeserializeObject(source);
            context.Body = body;
        }
    }
}
