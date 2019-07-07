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

using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.UI.Internal
{
	public sealed class InvokeUiData : UiData
	{
		public override TModel GetModel<TService, TModel>(string template, Ui serviceProvider)
		{
			var model = GetModel<TService>(template, serviceProvider);
			if (model == null)
				return default;
			return (TModel) model;
		}

		public override object GetModel<TService>(string template, Ui ui)
		{
			return GetModel(template, typeof(TService), ui);
		}

		public override object GetModel(string template, Type serviceType, Ui ui)
		{
			return PopulateAndExecute(ui, null, serviceType, template);
		}

		public override object GetModel(string template, MethodInfo method, Ui ui)
		{
			return PopulateAndExecute(ui, method, method.DeclaringType, template);
		}

		private static object PopulateAndExecute(Ui ui, MethodInfo callee, Type serviceType, string template)
		{
			var serviceProvider = ui.Context.UiServices;

			var settings = serviceProvider.GetRequiredService<UiSettings>();
			var layoutRoot = serviceProvider.GetRequiredService<LayoutRoot>();

			var target = Pools.AutoResolver.AutoResolve(serviceType, serviceProvider);
			var action = Pools.ActionPool.Get();
			try
			{
				layoutRoot.Systems.TryGetValue(template, out var system);
				system = system ?? settings.DefaultSystem ??
				         throw new ArgumentException(
					         "No registered system for the given template, and no default system to fall back on");

				system.PopulateAction(ui, settings, action, template, target, callee);

				if (layoutRoot.Filters.TryGetValue("*", out var filter)) filter(callee, ui.Context);

				if (layoutRoot.Filters.TryGetValue(template, out filter)) filter(callee, ui.Context);

				var result = action.Arguments == null
					? target.ExecuteMethod(action.MethodName)
					: target.ExecuteMethod(action.MethodName, action.Arguments);

				return result;
			}
			finally
			{
				Pools.ActionPool.Return(action);
			}
		}
	}
}