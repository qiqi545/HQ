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
using HQ.Cadence.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Cadence.AspNetCore.Mvc
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MeterAttribute : ActionFilterAttribute
    {
        private readonly long _eventOccurrences;
        private readonly string _eventType;
        private readonly TimeUnit _rateUnit;

        public MeterAttribute(string eventType, TimeUnit rateUnit, long eventOccurrences = 1L, string name = null,
            Type owner = null)
        {
            _eventType = eventType;
            _rateUnit = rateUnit;
            _eventOccurrences = eventOccurrences;
            Name = name;
            Owner = owner;
        }

        public string Name { get; set; }
        public Type Owner { get; set; }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var type = Owner ?? filterContext.GetMetricOwner();
            var name = Name ?? filterContext.GetMetricName<MeterMetric>();

            var metricsHost =
                filterContext.HttpContext.RequestServices.GetService(typeof(IMetricsHost)) as IMetricsHost;
            var meter = metricsHost?.Meter(type, name, _eventType, _rateUnit);
            meter?.Mark(_eventOccurrences);
        }
    }
}
