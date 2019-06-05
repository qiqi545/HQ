// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Lime.Internal
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

				if (layoutRoot.Filters.TryGetValue("*", out var filter))
				{
					filter(ui.Context);
				}

				if (layoutRoot.Filters.TryGetValue(template, out filter))
				{
					filter(ui.Context);
				}

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