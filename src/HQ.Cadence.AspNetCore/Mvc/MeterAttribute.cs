// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using HQ.Cadence.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Cadence.AspNetCore.Mvc
{
	[AttributeUsage(AttributeTargets.Method)]
	public class MeterAttribute : ActionFilterAttribute
	{
		readonly string _eventType;
		readonly TimeUnit _rateUnit;
		readonly long _eventOccurrences;

		public string Name { get; set; }
		public Type Owner { get; set; }

		public MeterAttribute(string eventType, TimeUnit rateUnit, long eventOccurrences = 1L, string name = null, Type owner = null)
		{
			_eventType = eventType;
			_rateUnit = rateUnit;
			_eventOccurrences = eventOccurrences;
			Name = name;
			Owner = owner;
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var type = Owner ?? filterContext.GetMetricOwner();
			var name = Name ?? filterContext.GetMetricName<MeterMetric>();

			var metricsHost = filterContext.HttpContext.RequestServices.GetService(typeof(IMetricsHost)) as IMetricsHost;
			var meter = metricsHost?.Meter(type, name, _eventType, _rateUnit);
			meter?.Mark(_eventOccurrences);
		}
	}
}