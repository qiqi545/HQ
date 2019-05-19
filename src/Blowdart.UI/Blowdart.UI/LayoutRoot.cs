// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;

namespace Blowdart.UI
{
    public class LayoutRoot
    {
	    private readonly Dictionary<string, NameValueCollection> _meta = new Dictionary<string, NameValueCollection>();
	    private readonly Dictionary<string, Action<Ui>> _handlers = new Dictionary<string, Action<Ui>>();
	    private readonly Dictionary<string, UiSystem> _systems = new Dictionary<string, UiSystem>();

		public LayoutRoot(IServiceProvider serviceProvider)
        {
            Services = serviceProvider;
        }

        internal IReadOnlyDictionary<string, NameValueCollection> Meta => _meta;
		internal IReadOnlyDictionary<string, Action<Ui>> Handlers => _handlers;
		internal IReadOnlyDictionary<string, UiSystem> Systems => _systems;

		internal IServiceProvider Services { get; }
        public Action<Ui> Root => Handlers["/"];

        #region Default

        public LayoutRoot Default(Action<Ui> view)
        {
            return Template("/", view);
        }

        public LayoutRoot Default<TService>(Action<Ui, dynamic> view)
        {
            return Template<TService>("/", view);
        }

        public LayoutRoot Default<TService, TModel>(Action<Ui, TModel> view) where TModel : class
        {
            return Template<TService, TModel>("/", view);
        }

        #endregion

        #region Template

        public LayoutRoot Template(string template, Action<Ui> view)
        {
            _handlers.Add(Normalize(template), view);
            return this;
        }

        public LayoutRoot Template<TService>(string template, Action<Ui, dynamic> view)
        {
			_handlers.Add(Normalize(template), ui =>
            {
                view(ui, ui.Data.GetModel<TService>(template));
            });
            return this;
        }

        public LayoutRoot Template<TService, TModel>(string template, Action<Ui, TModel> view) where TModel : class
        {
	        _handlers.Add(Normalize(template), ui =>
            {
                view(ui, ui.Data.GetModel<TService, TModel>(template));
            });
            return this;
        }

        private static string Normalize(string template)
        {
	        return !template.StartsWith("/") ? $"/{template}" : template;
        }

		#endregion

		internal LayoutRoot AddHandler(string template, MethodInfo method)
		{
			return Template(template, ui =>
			{
				ui.Data.GetModel(template, method, ui); // invoke-only: view and the model are one (IMGUI)
			});
		}

		internal LayoutRoot AddMeta(string template, NameValueCollection meta)
		{
			_meta.Add(template, meta);
			return this;
		}

		internal LayoutRoot AddSystem(string template, UiSystem system)
		{
			_systems.Add(template, system);
			return this;
		}
    }
}