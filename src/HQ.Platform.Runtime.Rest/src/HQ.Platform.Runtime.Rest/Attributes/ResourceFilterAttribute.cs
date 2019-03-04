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

using System.Net;
using HQ.Data.Contracts;
using HQ.Data.Contracts.AspNetCore.Mvc;
using HQ.Data.Contracts.Runtime;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;

namespace HQ.Platform.Runtime.Rest.Attributes
{
    public class ResourceFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var qs = QueryHelpers.ParseQuery(context.HttpContext.Request.QueryString.Value);
            var qc = new QueryContext();

            if (context.ActionArguments.ContainsKey("page"))
            {
                PageFilterAttribute.Execute(context, qs, qc);
            }

            if (context.ActionArguments.ContainsKey("stream"))
            {
                StreamFilterAttribute.Execute(context, qs, qc);
            }

            if (context.ActionArguments.ContainsKey("sort"))
            {
                SortFilterAttribute.Execute(context, qs, qc);
            }

            if (context.ActionArguments.ContainsKey("fields"))
            {
                FieldsFilterAttribute.Execute(context, qs, qc);
            }

            if (context.ActionArguments.ContainsKey("filter"))
            {
                FilterFilterAttribute.Execute(context, qs, qc);
            }

            if (context.ActionArguments.ContainsKey("projection"))
            {
                ProjectionFilterAttribute.Execute(context, qs, qc);
            }

            if (qc.Errors?.Count > 0)
            {
                context.Result = new ErrorResult(new Error(ErrorEvents.ValidationFailed,
                    ErrorStrings.ValidationFailed, (HttpStatusCode) 422, qc.Errors));
            }
        }
    }
}
