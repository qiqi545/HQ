#region LICENSE

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

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;

namespace HQ.Common.AspNetCore
{
	public class DynamicViewLocationExpander<T> : IViewLocationExpander
	{
		private const string ContextKey = Constants.ContextKeys.DynamicViewLocation;
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
