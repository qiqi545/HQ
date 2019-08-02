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
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Extensions.Metrics.AspNetCore.Internal
{
    internal static class ActionExecutedContextExtensions
    {
        public static Type GetMetricOwner(this ActionExecutingContext filterContext)
        {
            return filterContext.Controller.GetType();
        }

        public static string GetMetricName<T>(this ActionExecutingContext filterContext) where T : IMetric
        {
            return $"{filterContext.ActionDescriptor.RouteValues["action"]}.{typeof(T).Name}";
        }

        public static Type GetMetricOwner(this ActionExecutedContext filterContext)
        {
            return filterContext.Controller.GetType();
        }

        public static string GetMetricName<T>(this ActionExecutedContext filterContext) where T : IMetric
        {
            return $"{filterContext.ActionDescriptor.RouteValues["action"]}.{typeof(T).Name}";
        }
    }
}
