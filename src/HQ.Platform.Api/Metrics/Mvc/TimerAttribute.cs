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
using System.Diagnostics;
using HQ.Platform.Api.Metrics.Internal;
using Metrics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Platform.Api.Metrics.Mvc
{
	[AttributeUsage(AttributeTargets.Method)]
	public class TimerAttribute : ActionFilterAttribute
	{
		private readonly TimeUnit _durationUnit;
		private readonly TimeUnit _rateUnit;

		public TimerAttribute(TimeUnit durationUnit, TimeUnit rateUnit, string name = null, Type owner = null)
		{
			_durationUnit = durationUnit;
			_rateUnit = rateUnit;
			Name = name;
			Owner = owner;
		}

		public string Name { get; set; }
		public Type Owner { get; set; }

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var type = Owner ?? filterContext.GetMetricOwner();
			var name = Name ?? filterContext.GetMetricName<TimerMetric>();

			filterContext.HttpContext.Items[$"{type}.{name}"] = Stopwatch.StartNew();
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var type = Owner ?? filterContext.GetMetricOwner();
			var name = Name ?? filterContext.GetMetricName<TimerMetric>();

			var metricsHost =
				filterContext.HttpContext.RequestServices.GetService(typeof(IMetricsHost)) as IMetricsHost;
			var timer = metricsHost?.Timer(type, name, _durationUnit, _rateUnit);

			if (filterContext.HttpContext.Items[$"{type}.{name}"] is Stopwatch stopwatch)
			{
				stopwatch?.Stop();
				timer?.Update(stopwatch.Elapsed.Ticks, _durationUnit);
			}
		}
	}
}