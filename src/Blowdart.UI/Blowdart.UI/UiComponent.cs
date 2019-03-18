// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Blowdart.UI.Internal;

namespace Blowdart.UI
{
    public abstract class UiComponent
    {
        public virtual string Name => null;
        public virtual void Render(Ui ui) { Render(ui, null); }

        public virtual void Render(Ui ui, dynamic model)
        {
            var componentType = GetType();
            if (componentType.BaseType != null && componentType.BaseType.IsConstructedGenericType)
            {
                var modelType = componentType.BaseType.GetGenericArguments()[0];
                var getMethod = new Func<MethodInfo>(() => componentType.GetMethod(nameof(Render), new[] { typeof(Ui), modelType }));
                TypeExtensions.ExecuteMethodFunction(this, $"{componentType.Name}_{nameof(Render)}_{modelType.Name}", getMethod, ui, model);
            }
            else
            {
                throw new NotSupportedException("You need to override `Render(Ui ui, dynamic model)` to call it in this way.");
            }
        }
    }

    /// <inheritdoc />
    public abstract class UiComponent<TModel> : UiComponent
    {
        public abstract void Render(Ui ui, TModel model);
    }
}