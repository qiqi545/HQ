// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace Blowdart.UI.Internal
{
    public class InvokeUiData : UiData
    {
        private static readonly ObjectPool<UiAction> ActionPool = new DefaultObjectPool<UiAction>(new DefaultPooledObjectPolicy<UiAction>());

        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceProvider _autoResolver;

        public InvokeUiData(IServiceProvider serviceProvider, IEnumerable<Assembly> fallbackAssemblies)
        {
            _serviceProvider = serviceProvider;
            _autoResolver = new NoContainer(serviceProvider, fallbackAssemblies);
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
            var settings = _serviceProvider.GetRequiredService<UiSettings>();
            var target = _autoResolver.GetService<TService>();

            var action = ActionPool.Get();

            try
            {
                settings.System.PopulateAction(settings, action, _autoResolver, template, target);

                return action.Arguments == null
                    ? target.ExecuteMethod(action.MethodName)
                    : target.ExecuteMethod(action.MethodName, action.Arguments);
            }
            finally
            {
                action.Arguments = null;
                action.MethodName = null;
                ActionPool.Return(action);
            }
        }
    }
}
