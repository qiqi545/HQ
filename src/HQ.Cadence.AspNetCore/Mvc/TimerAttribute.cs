// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Cadence.AspNetCore.Mvc
{
	[AttributeUsage(AttributeTargets.Method)]
	public class TimerAttribute : ActionFilterAttribute
	{
		readonly TimeUnit _durationUnit;
		readonly TimeUnit _rateUnit;
		readonly Type _owner;
		readonly string _name;

		public TimerAttribute(TimeUnit durationUnit, TimeUnit rateUnit, string name = null, Type owner = null)
		{
			_durationUnit = durationUnit;
			_rateUnit = rateUnit;
			_name = name;
			_owner = owner;
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var type = _owner ?? filterContext.GetMetricOwner();
			var name = _name ?? filterContext.GetMetricName<TimerMetric>();

			filterContext.HttpContext.Items[$"{type}.{name}"] = Stopwatch.StartNew();
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var type = _owner ?? filterContext.GetMetricOwner();
			var name = _name ?? filterContext.GetMetricName<TimerMetric>();

			var metricsHost = filterContext.HttpContext.RequestServices.GetService(typeof(IMetricsHost)) as IMetricsHost;
			var timer = metricsHost?.Timer(type,  name, _durationUnit, _rateUnit);

			if (filterContext.HttpContext.Items[$"{type}.{name}"] is Stopwatch stopwatch)
			{
				stopwatch?.Stop();
				timer?.Update(stopwatch.Elapsed.Ticks, _durationUnit);
			}
		}
	}
}