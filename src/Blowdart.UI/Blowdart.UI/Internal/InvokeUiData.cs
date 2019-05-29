// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Blowdart.UI.Internal
{
    public sealed class InvokeUiData : UiData
    {
        public override TModel GetModel<TService, TModel>(string template, IServiceProvider serviceProvider)
        {
            var model = GetModel<TService>(template, serviceProvider);
            if (model == null)
                return default;
            return (TModel) model;
        }

        public override object GetModel<TService>(string template, IServiceProvider serviceProvider)
        {
            return GetModel(template, typeof(TService), serviceProvider);
        }

        public override object GetModel(string template, Type serviceType, IServiceProvider serviceProvider)
        {
            return PopulateAndExecute(template, serviceType, null, null, serviceProvider);
        }

        public override object GetModel(string template, MethodInfo method, Ui ui)
        {
            return PopulateAndExecute(template, method.DeclaringType, method, ui, ui.Context.UiServices);
        }

        private static object PopulateAndExecute(string template, Type serviceType, MethodInfo callee, Ui ui, IServiceProvider serviceProvider)
        {
            var settings = serviceProvider.GetRequiredService<UiSettings>();
            var layoutRoot = serviceProvider.GetRequiredService<LayoutRoot>();
			
			var target = Pools.AutoResolver.GetService(serviceType);
            var action = Pools.ActionPool.Get();
            try
            {
	            layoutRoot.Systems.TryGetValue(template, out var system);
	            system = system ?? settings.DefaultSystem ??
	                     throw new ArgumentException("No registered system for the given template, and no default system to fall back on");
				
	            system.PopulateAction(settings, action, Pools.AutoResolver, template, target, callee, ui);

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
