// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Cadence.AspNetCore.Internal
{
	internal static class ActionExecutedContextExtensions
	{
		public static Type GetMetricOwner(this ActionExecutingContext filterContext)
		{
			return filterContext.Controller.GetType();
		}

		public static string GetMetricName<T>(this ActionExecutingContext filterContext) where T : IMetric
		{
			return filterContext.ActionDescriptor.RouteValues["action"] + "." + typeof(T).Name;
		}

		public static Type GetMetricOwner(this ActionExecutedContext filterContext)
		{
			return filterContext.Controller.GetType();
		}

		public static string GetMetricName<T>(this ActionExecutedContext filterContext) where T : IMetric
		{
			return filterContext.ActionDescriptor.RouteValues["action"] + "." + typeof(T).Name;
		}
	}
}