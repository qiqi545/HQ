﻿#region LICENSE

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
using HQ.Platform.Api.Metrics.Internal;
using Metrics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Platform.Api.Metrics.Mvc
{
	[AttributeUsage(AttributeTargets.Method)]
	public class HistogramAttribute : ActionFilterAttribute
	{
		private readonly SampleType _sampleType;
		private readonly long _value;

		public HistogramAttribute(SampleType sampleType, long value, string name = null, Type owner = null)
		{
			_sampleType = sampleType;
			_value = value;
			Name = name;
			Owner = owner;
		}

		public string Name { get; set; }
		public Type Owner { get; set; }

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var type = Owner ?? filterContext.GetMetricOwner();
			var name = Name ?? filterContext.GetMetricName<HistogramMetric>();

			var metricsHost =
				filterContext.HttpContext.RequestServices.GetService(typeof(IMetricsHost)) as IMetricsHost;
			var histogram = metricsHost?.Histogram(type, name, _sampleType);
			histogram?.Update(_value);
		}
	}
}