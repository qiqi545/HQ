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

        internal IDictionary<string, Action<Ui>> Handlers { get; } = new Dictionary<string, Action<Ui>>();
        internal IServiceProvider Services { get; }

        public LayoutRoot Default(Action<Ui> view)
        {
            return Template(nameof(Default), view);
        }

        public LayoutRoot Default<TService>(Action<Ui, dynamic> view)
        {
            return Template<TService>("/", view);
        }

        public LayoutRoot Default<TService, TModel>(Action<Ui, TModel> view) where TModel : class
        {
            return Template<TService, TModel>("/", view);
        }

        public LayoutRoot Template(string template, Action<Ui> view)
        {
            Handlers.Add(template, view);
            return this;
        }

        public LayoutRoot Template<TService>(string template, Action<Ui, dynamic> view)
        {
            Handlers.Add(template, ui =>
            {
                view(ui, ui.Data.GetModel<TService>(template));
            });
            return this;
        }

        public LayoutRoot Template<TService, TModel>(string template, Action<Ui, TModel> view) where TModel : class
        {
            Handlers.Add(template, ui =>
            {
                view(ui, ui.Data.GetModel<TService, TModel>(template));
            });
            return this;
        }

        public Action<Ui> Root => Handlers["/"];
    }
}