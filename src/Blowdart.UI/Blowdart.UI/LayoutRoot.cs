// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Blowdart.UI
{
    public class LayoutRoot
    {
        public LayoutRoot(IServiceProvider serviceProvider)
        {
            Services = serviceProvider;
        }

        internal Action<Ui> Root { get; set; }
        internal IDictionary<string, Action<Ui>> Routes { get; } = new Dictionary<string, Action<Ui>>();

        internal IServiceProvider Services { get; }

        public LayoutRoot Default(Action<Ui> view)
        {
            Root = view;
            return this;
        }
    }
}