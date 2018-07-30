// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Cadence.AspNetCore.Mvc
{
	[AttributeUsage(AttributeTargets.Method)]
	public class HistogramAttribute : ActionFilterAttribute
	{
		readonly SampleType _sampleType;
		readonly long _value;
		readonly Type _owner;
		readonly string _name;

		public HistogramAttribute(SampleType sampleType, long value, string name = null, Type owner = null)
		{
			_sampleType = sampleType;
			_value = value;
			_name = name;
			_owner = owner;
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var type = _owner ?? filterContext.GetMetricOwner();
			var name = _name ?? filterContext.GetMetricName<HistogramMetric>();

			var metricsHost = filterContext.HttpContext.RequestServices.GetService(typeof(IMetricsHost)) as IMetricsHost;
			var histogram = metricsHost?.Histogram(type, name, _sampleType);
			histogram?.Update(_value);
		}
	}
}