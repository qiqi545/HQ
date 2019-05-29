// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Blowdart.UI
{
    public abstract class UiData
    {
        public abstract TModel GetModel<TController, TModel>(string template, IServiceProvider serviceProvider);
        public abstract object GetModel<TController>(string template, IServiceProvider serviceProvider);
        public abstract object GetModel(string template, Type serviceType, IServiceProvider serviceProvider);
        public abstract object GetModel(string template, MethodInfo method, Ui ui);
    }
}