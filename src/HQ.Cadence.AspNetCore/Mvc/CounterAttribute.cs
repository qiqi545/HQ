// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Cadence.AspNetCore.Mvc
{
	[AttributeUsage(AttributeTargets.Method)]
    public class CounterAttribute : ActionFilterAttribute
	{
		readonly long _incrementBy;
		readonly Type _owner;
		readonly string _name;

		public CounterAttribute(long incrementBy = 1L, Type owner = null, string name = null)
		{
			_incrementBy = incrementBy;
			_owner = owner;
			_name = name;
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var metricsHost = filterContext.HttpContext.RequestServices.GetService(typeof(IMetricsHost)) as IMetricsHost;
			var counter = metricsHost?.Counter(_owner ?? filterContext.GetMetricOwner(), _name ?? filterContext.GetMetricName<CounterMetric>());
			counter?.Increment(_incrementBy);
		}
	}
}