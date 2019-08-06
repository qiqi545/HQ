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

using System.Reflection;
using HQ.Data.Contracts.Configuration;
using HQ.Data.Contracts.Mvc;
using HQ.Data.Contracts.Runtime;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Api.Runtime.Rest.Attributes
{
    public class ChildResourceFilterAttribute : ActionFilterAttribute
    {
        private readonly IOptions<QueryOptions> _options;

        public ChildResourceFilterAttribute(IOptions<QueryOptions> options)
        {
            _options = options;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.Controller is IObjectController controller))
            {
                return;
            }

            var resourceType = context.ActionDescriptor is ControllerActionDescriptor descriptor
                ? descriptor.MethodInfo.GetCustomAttribute(typeof(ObjectTypeAttribute)) is ObjectTypeAttribute
                    objectTypeAttribute ? objectTypeAttribute.Type : controller.ObjectType
                : controller.ObjectType;

            var qc = new QueryContext(context.HttpContext.User) {Type = resourceType};

            var qs = QueryHelpers.ParseQuery(context.HttpContext.Request.QueryString.Value);

            if (context.Result == null)
            {
                PageFilterAttribute.Execute(context, qs, qc);
            }

            if (context.Result == null)
            {
                SortFilterAttribute.Execute(context, qs, qc);
            }

            if (context.Result == null)
            {
                FieldsFilterAttribute.Execute(context, qs, qc);
            }

            if (context.Result == null)
            {
                FilterFilterAttribute.Execute(context, qs, qc);
            }

            if (context.Result == null)
            {
                ProjectionFilterAttribute.Execute(context, qs, qc);
            }
        }
    }
}
