// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Blowdart.UI.Internal
{
    public class InvokeUiData : UiData
    {
        private readonly IServiceProvider _serviceProvider;

        public InvokeUiData(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override TModel GetModel<TService, TModel>(string methodName)
        {
            return GetModel<TService>(methodName) as TModel;
        }

        public override object GetModel<TService>(string methodName)
        {
            var service = _serviceProvider.GetService(typeof(TService));
            var model = service?.GetType().ExecuteMethod(methodName);
            return model;
        }
    }
}
