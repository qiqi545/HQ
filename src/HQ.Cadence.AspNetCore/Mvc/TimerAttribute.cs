// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Diagnostics;
using HQ.Cadence.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Cadence.AspNetCore.Mvc
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