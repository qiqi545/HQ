// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Blowdart.UI.Internal
{
    public class InvokeUiData : UiData
    {
        private readonly IServiceProvider _serviceProvider;

        public InvokeUiData(IServiceProvider serviceProvider, IEnumerable<Assembly> fallbackAssemblies)
        {
            _serviceProvider = new NoContainer(serviceProvider, fallbackAssemblies);
        }

        public override TModel GetModel<TService, TModel>(string methodName)
        {
            var serviceType = typeof(TService);
            var service = _serviceProvider.GetService(serviceType);
            if (service == null)
                return default;

            var model = service.ExecuteMethod(methodName);
            return model as TModel;
        }
    }
}
