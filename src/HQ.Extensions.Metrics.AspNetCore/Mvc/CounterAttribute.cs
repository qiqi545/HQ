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
using HQ.Extensions.Metrics.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Extensions.Metrics.AspNetCore.Mvc
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CounterAttribute : ActionFilterAttribute
    {
        private readonly long _incrementBy;

        public CounterAttribute(long incrementBy = 1L, string name = null, Type owner = null)
        {
            _incrementBy = incrementBy;
            Name = name;
            Owner = owner;
        }

        public string Name { get; set; }
        public Type Owner { get; set; }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var metricsHost =
                filterContext.HttpContext.RequestServices.GetService(typeof(IMetricsHost)) as IMetricsHost;
            var counter = metricsHost?.Counter(Owner ?? filterContext.GetMetricOwner(),
                Name ?? filterContext.GetMetricName<CounterMetric>());
            counter?.Increment(_incrementBy);
        }
    }
}
