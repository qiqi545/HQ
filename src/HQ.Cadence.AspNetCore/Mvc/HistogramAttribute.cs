// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using HQ.Cadence.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Cadence.AspNetCore.Mvc
{
	[AttributeUsage(AttributeTargets.Method)]
	public class HistogramAttribute : ActionFilterAttribute
	{
		readonly SampleType _sampleType;
		readonly long _value;

		public string Name { get; set; }
		public Type Owner { get; set; }

		public HistogramAttribute(SampleType sampleType, long value, string name = null, Type owner = null)
		{
			_sampleType = sampleType;
			_value = value;
			Name = name;
			Owner = owner;
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var type = Owner ?? filterContext.GetMetricOwner();
			var name = Name ?? filterContext.GetMetricName<HistogramMetric>();

			var metricsHost = filterContext.HttpContext.RequestServices.GetService(typeof(IMetricsHost)) as IMetricsHost;
			var histogram = metricsHost?.Histogram(type, name, _sampleType);
			histogram?.Update(_value);
		}
	}
}