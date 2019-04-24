// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Blowdart.UI.Internal
{
    public class InvokeUiData : UiData
    {
        private readonly IServiceProvider _serviceProvider;
        
        public InvokeUiData(IServiceProvider serviceProvider, IEnumerable<Assembly> fallbackAssemblies)
        {
            _serviceProvider = serviceProvider;
        }

        public override TModel GetModel<TService, TModel>(string template)
        {
            var model = GetModel<TService>(template);
            if (model == null)
                return default;
            return (TModel) model;
        }

        public override object GetModel<TService>(string template)
        {
            return GetModel(template, typeof(TService));
        }

        public override object GetModel(string template, Type serviceType)
        {
            return PopulateAndExecute(template, serviceType, null, null);
        }

        public override object GetModel(string template, MethodInfo method, Ui ui)
        {
            return PopulateAndExecute(template, method.DeclaringType, method, ui);
        }

        private object PopulateAndExecute(string template, Type serviceType, MethodInfo callee, Ui ui)
        {
            var settings = _serviceProvider.GetRequiredService<UiSettings>();
            var target = Pools.AutoResolver.GetService(serviceType);
            var action = Pools.ActionPool.Get();
            try
            {
				settings.System.PopulateAction(settings, action, Pools.AutoResolver, template, target, callee, ui);

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
