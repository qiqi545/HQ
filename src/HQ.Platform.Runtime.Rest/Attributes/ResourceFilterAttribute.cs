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

using HQ.Data.Contracts;
using HQ.Data.Contracts.Configuration;
using HQ.Data.Contracts.Mvc;
using HQ.Data.Contracts.Runtime;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Runtime.Rest.Attributes
{
    public class ResourceFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<QueryOptions>>();

            var qs = QueryHelpers.ParseQuery(context.HttpContext.Request.QueryString.Value);
            var qc = new QueryContext(context.HttpContext.User);

            if (context.ActionArguments.ContainsKey(options.Value.PageOperator))
            {
                PageFilterAttribute.Execute(context, qs, qc);
            }

            if (context.ActionArguments.ContainsKey("stream"))
            {
                StreamFilterAttribute.Execute(context, qs, qc);
            }

            if (context.ActionArguments.ContainsKey(options.Value.SortOperator))
            {
                SortFilterAttribute.Execute(context, qs, qc);
            }

            if (context.ActionArguments.ContainsKey(options.Value.FieldsOperator))
            {
                FieldsFilterAttribute.Execute(context, qs, qc);
            }

            if (context.ActionArguments.ContainsKey(options.Value.FilterOperator))
            {
                FilterFilterAttribute.Execute(context, qs, qc);
            }

            if (context.ActionArguments.ContainsKey(options.Value.ProjectionOperator))
            {
                ProjectionFilterAttribute.Execute(context, qs, qc);
            }

            if (qc.Errors?.Count > 0)
            {
                context.Result = new ErrorResult(new Error(ErrorEvents.ValidationFailed,
                    ErrorStrings.ValidationFailed, 422 /*HttpStatusCode.UnprocessableEntity*/, qc.Errors));
            }
        }
    }
}
