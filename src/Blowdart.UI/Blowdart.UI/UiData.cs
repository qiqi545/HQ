// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Blowdart.UI
{
    public abstract class UiData
    {
        public abstract TModel GetModel<TService, TModel>(string methodName) where TModel : class;
    }
}