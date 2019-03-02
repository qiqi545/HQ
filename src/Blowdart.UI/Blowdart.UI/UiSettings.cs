// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Blowdart.UI
{
    public class UiSettings
    {
        public UiSettings(IServiceProvider serviceProvider)
        {
            Title = "My Blowdart UI";
            Services = serviceProvider;
        }

        public IServiceProvider Services { get; }

        public string Title { get; set; }
        public UiSystem System { get; set; }
        public Assembly[] ComponentAssemblies { get; set; }
    }
}