// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Blowdart.UI
{
    public abstract class UiSystem
    {
        public abstract void Begin();
        public abstract void End();

        public virtual void Error(string errorMessage, Exception exception = null)
        {
            Trace.WriteLine($"UI error: {errorMessage} {(exception == null ? "" : $"{exception}")}");
        }

        public virtual void PopulateAction(UiAction action, IServiceProvider serviceProvider, string template, object target)
        {
            action.MethodName = template;
            action.Arguments = null;
        }
    }
}