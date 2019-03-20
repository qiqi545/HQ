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
            return Template(nameof(Default), view);
        }

        public LayoutRoot Default<TService>(Action<Ui, dynamic> view)
        {
            return Template<TService>(nameof(Default), view);
        }

        public LayoutRoot Default<TService, TModel>(Action<Ui, TModel> view) where TModel : class
        {
            return Template<TService, TModel>(nameof(Default), view);
        }

        public LayoutRoot Template(string template, Action<Ui> view)
        {
            Root = view;
            return this;
        }

        public LayoutRoot Template<TService>(string template, Action<Ui, dynamic> view)
        {
            Root = ui =>
            {
                view(ui, ui.Data.GetModel<TService>(template));
            };
            return this;
        }

        public LayoutRoot Template<TService, TModel>(string template, Action<Ui, TModel> view) where TModel : class
        {
            Root = ui =>
            {
                view(ui, ui.Data.GetModel<TService, TModel>(template));
            };
            return this;
        }
    }
}