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
using System.Security.Claims;
using HQ.Data.Contracts.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace HQ.Platform.Runtime.Rest.Models
{
    public class RestQueryContextProvider : IQueryContextProvider
    {
        private readonly IEnumerable<IRestFilter> _filters;

        public RestQueryContextProvider(IEnumerable<IRestFilter> filters)
        {
            _filters = filters;
        }

        public IEnumerable<QueryContext> Parse(HttpContext source)
        {
            return Parse(source.User, source.Request.QueryString.Value);
        }

        public IEnumerable<QueryContext> Parse(ClaimsPrincipal user, string source)
        {
            var context = new QueryContext(user);

            // context.Type = ...

            BuildHandleData(context, source);

            // context.Handle = ...

            yield return context;
        }

        private void BuildHandleData(QueryContext context, string source)
        {
            var qs = QueryHelpers.ParseQuery(source);
            foreach (var filter in _filters)
            {
                filter?.Execute(qs, ref context);
            }
        }
    }
}
