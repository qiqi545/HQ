#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using TypeKitchen;

namespace HQ.UI
{
	public class LayoutRoot
	{
		private const string ForwardSlash = "/";

		private readonly Dictionary<string, Action<MethodInfo, UiContext>> _filters =
			new Dictionary<string, Action<MethodInfo, UiContext>>();

		private readonly Dictionary<string, Action<Ui>> _handlers = new Dictionary<string, Action<Ui>>();

		private readonly Dictionary<string, NameValueCollection> _meta = new Dictionary<string, NameValueCollection>();
		private readonly Dictionary<string, UiSystem> _systems = new Dictionary<string, UiSystem>();

		internal IReadOnlyDictionary<string, NameValueCollection> Meta => _meta;
		internal IReadOnlyDictionary<string, Action<Ui>> Handlers => _handlers;
		internal IReadOnlyDictionary<string, UiSystem> Systems => _systems;
		internal IReadOnlyDictionary<string, Action<MethodInfo, UiContext>> Filters => _filters;

		internal Action<Ui> Root => Handlers[ForwardSlash];

		public LayoutRoot AddGlobalFilter(Action<MethodInfo, UiContext> filter)
		{
			_filters["*"] = filter;
			return this;
		}

		public LayoutRoot AddFilter(string template, Action<MethodInfo, UiContext> filter)
		{
			_filters[template] = filter;
			return this;
		}

		public LayoutRoot AddHandler(string template, MethodInfo method)
		{
			return Template(template, ui =>
			{
				ui.Data.GetModel(template, method, ui); // invoke-only: view and the model are one (IMGUI)
			});
		}

		public LayoutRoot AddMeta(string template, NameValueCollection meta)
		{
			_meta.Add(Normalize(template), meta);
			return this;
		}

		public LayoutRoot AddSystem(string template, UiSystem system)
		{
			_systems.Add(Normalize(template), system);
			return this;
		}

		private static string Normalize(string template)
		{
			if (string.IsNullOrWhiteSpace(template) || template.Equals(ForwardSlash, StringComparison.Ordinal))
				return ForwardSlash;

			return Pooling.StringBuilderPool.Scoped(sb =>
			{
				if (!template.StartsWith(ForwardSlash, StringComparison.Ordinal))
					sb.Append('/');

				if (template.EndsWith(ForwardSlash, StringComparison.Ordinal))
					sb.Append(template, 0, template.Length - 1);
				else
					sb.Append(template);
			});
		}

		#region Default

		public LayoutRoot Default(Action<Ui> view)
		{
			return Template(ForwardSlash, view);
		}

		public LayoutRoot Default<TService>(Action<Ui, dynamic> view)
		{
			return Template<TService>(ForwardSlash, view);
		}

		public LayoutRoot Default<TService, TModel>(Action<Ui, TModel> view) where TModel : class
		{
			return Template<TService, TModel>(ForwardSlash, view);
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
				view(ui, ui.Data.GetModel<TService>(template, ui));
			});
			return this;
		}

		public LayoutRoot Template<TService, TModel>(string template, Action<Ui, TModel> view) where TModel : class
		{
			_handlers.Add(Normalize(template), ui =>
			{
				view(ui, ui.Data.GetModel<TService, TModel>(template, ui));
			});
			return this;
		}

		#endregion
	}
}