// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using HQ.Cadence.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Cadence.AspNetCore.Mvc
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