// Copyright (c) HQ.IO Corporation. All rights reserved.
// Usage is strictly forbidden if not under license.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;

namespace HQ.Common.AspNetCore
{
	public class DynamicViewLocationExpander<T> : IViewLocationExpander
	{
		private const string ContextKey = HqContextKeys.DynamicViewLocation;
		private readonly string _value;

		public DynamicViewLocationExpander()
		{
			_value = $"_{typeof(T).Name}";
		}

		public void PopulateValues(ViewLocationExpanderContext context)
		{
			var controller = context.ActionContext.ActionDescriptor.RouteValues["controller"];
			if (controller.EndsWith(_value))
			{
				var viewLocation = controller.Replace(_value, string.Empty);
				context.Values[ContextKey] = $"~/Views/{viewLocation}/{context.ViewName}.cshtml";
			}
		}

		public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
			IEnumerable<string> viewLocations)
		{
			if (context.Values.TryGetValue(ContextKey, out var dynamicViewLocation))
				yield return dynamicViewLocation;

			foreach (var viewLocation in viewLocations)
				yield return viewLocation;
		}
	}
}