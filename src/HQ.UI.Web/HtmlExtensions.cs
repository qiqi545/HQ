﻿#region LICENSE

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
using System.Text;
using AngleSharp;
using AngleSharp.Html.Parser;
using HQ.UI.Web.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.UI.Web
{
	public static partial class HtmlExtensions
	{
		private static byte _indentLevel;
		private static readonly Stack<string> Elements = new Stack<string>();

		public static Ui Html(this Ui ui, string relativePath)
		{
			var env = ui.Context.UiServices.GetRequiredService<IHostingEnvironment>();
			var fileInfo = env.WebRootFileProvider.GetFileInfo(relativePath);
			if (fileInfo.Exists)
			{
				var parser = new HtmlParser(new HtmlParserOptions());
				var document = parser.ParseDocument(fileInfo.CreateReadStream());
				var web = ui.WebSystem();
				web.Document(document.ToHtml());
			}

			return ui;
		}

		public static Ui Literal(this Ui ui, string text)
		{
			return ui._(text);
		}

		public static Ui _(this Ui ui, string text)
		{
			Dom(ui).Tab();
			Dom(ui).Append(text);
			return ui;
		}

		public static Ui Break(this Ui ui)
		{
			return ui.br();
		}

		// ReSharper disable once InconsistentNaming
		public static Ui br(this Ui ui)
		{
			Dom(ui).Tab();
			Dom(ui).AppendTag("br");
			return ui;
		}

		public static Ui BeginElement(this Ui ui, string el, object attr = null)
		{
			Dom(ui).Tab();
			Dom(ui).OpenBlock(el, attr == null ? null : Attr(attr));
			_indentLevel++;
			Elements.Push(el);
			return ui;
		}

		public static Ui Element(this Ui ui, string el)
		{
			return ui.Element(el, attr: null);
		}

		public static Ui Element(this Ui ui, string el, object attr)
		{
			Dom(ui).Tab();
			Dom(ui).OpenBlock(el, attr == null ? null : Attr(attr));
			_indentLevel++;
			Elements.Push(el);
			ui.EndElement(el);
			return ui;
		}

		public static Ui Element(this Ui ui, string el, Action action)
		{
			return ui.Element(el, null, action: action);
		}

		public static Ui Element(this Ui ui, string el, object attr, Action action)
		{
			Dom(ui).Tab();
			Dom(ui).OpenBlock(el, attr == null ? null : Attr(attr));
			_indentLevel++;
			Elements.Push(el);
			action();
			ui.EndElement(el);
			return ui;
		}

		public static Ui Element(this Ui ui, string el, Action<Ui> action)
		{
			ui.Element(el, null, () => action?.Invoke(ui));
			return ui;
		}

		public static Ui Element(this Ui ui, string el, object attr, Action<Ui> action)
		{
			ui.Element(el, attr, () => action?.Invoke(ui));
			return ui;
		}

		public static Ui EndElement(this Ui ui, string el)
		{
			if (Elements.Count == 0)
				throw new HtmlException(
					$"Attempted to close a {el} without any open elements. You have a nesting issue somewhere.");
			if (el != Elements.Peek())
				throw new HtmlException($"Attempted to close a {el} before closing an inner {Elements.Peek()}");
			_indentLevel--;
			Dom(ui).Tab();
			Dom(ui).CloseBlock(el, true);
			Elements.Pop();
			return ui;
		}

		public static Ui Element(this Ui ui, string el, string innerText, object attr = null)
		{
			Dom(ui).Tab();
			Dom(ui).AppendTag(el, innerText, attr == null ? null : Attr(attr));
			return ui;
		}

		public static Ui Styles(this Ui ui, string styles)
		{
			Styles(ui).AppendLine($"{styles}");
			return ui;
		}

		public static Ui OnReady(this Ui ui, string script)
		{
			Scripts(ui).AppendLine($"{script}");
			return ui;
		}

		internal static WebUiContext WebContext(this Ui ui)
		{
			if (!(ui.Context is WebUiContext context))
				throw new NotSupportedException(ErrorStrings.MustUseHtmlSystem);
			return context;
		}

		private static HtmlSystem WebSystem(this Ui ui)
		{
			if (!(ui.System is HtmlSystem system))
				throw new NotSupportedException(ErrorStrings.MustUseHtmlSystem);
			return system;
		}

		private static StringBuilder Styles(Ui ui)
		{
			return ui.WebSystem().Styles;
		}

		private static StringBuilder Dom(Ui ui)
		{
			return ui.WebSystem().Dom;
		}

		private static StringBuilder Scripts(Ui ui)
		{
			return ui.WebSystem().Scripts;
		}

		private static Attributes Attr(object attr)
		{
			return attr is Attributes direct ? direct : Attributes.Attr(attr);
		}

		internal static Attributes Attr(params object[] attr)
		{
			return Attributes.Attr(attr);
		}

		private static void Tab(this StringBuilder dom)
		{
			dom.Append(Environment.NewLine);
			for (var i = 0; i < _indentLevel; i++)
				dom.Append('\t');
		}

		public static Ui Input(this Ui ui, InputType inputType, object attr = null)
		{
			var attribute = Attributes.Attr(new {type = inputType.ToString().ToLowerInvariant()});
			var attributes = attr == null ? attribute : Attributes.Attr(attribute, attr);
			ui.Element("input", null, attributes);
			return ui;
		}

		public static int Range(this Ui ui, int min, int max, int defaultValue, object attr = null)
		{
			var attribute = Attributes.Attr(new {type = "range"});
			ui.NextId();
			var id = ui.NextIdHash;
			Dom(ui).AppendTag("input", id, null, attr == null ? attribute : Attributes.Attr(attribute, attr));
			Scripts(ui).AppendEvent("input", id);

			return ui.InputValues.TryGetValue(id, out var newValue)
				? ui.InputValues[id] = newValue
				: ui.InputValues[id] = defaultValue;
		}

		public static Ui Submit(this Ui ui, string label = "Submit", object attr = null)
		{
			return ui.Input(InputType.Submit, Attr(attr, new {value = label}));
		}

		#region DOM Events

		public static bool OnClick(this Ui ui)
		{
			return OnEvent(ui, MouseEvents.click);
		}

		public static bool OnEvent(this Ui ui, string eventType)
		{
			Scripts(ui).AppendEvent(eventType, ui.NextIdHash);
			return ui.Events.Contains(eventType, ui.NextIdHash);
		}

		public static Ui Button(this Ui ui, string innerText, Action<ButtonEvents, ButtonAttributes> events,
			object attr = null)
		{
			var id = ui.NextId();

			if (events != null)
			{
				var e = new ButtonEvents();
				var a = new ButtonAttributes();
				var d = new MouseEventData();

				events(e, a);

				if (ui.Events.Contains(MouseEvents.click, id))
					e.click?.Invoke(d);

				if (ui.Events.Contains(MouseEvents.mouseover, id))
					e.mouseover?.Invoke(d);

				if (ui.Events.Contains(MouseEvents.mouseout, id))
					e.mouseout?.Invoke(d);

				if (a.innerText != null)
					innerText = a.innerText;

				Scripts(ui).AppendEvent(MouseEvents.click, id);
				Scripts(ui).AppendEvent(MouseEvents.mouseover, id);
				Scripts(ui).AppendEvent(MouseEvents.mouseout, id);
			}

			Dom(ui).AppendTag("button", id, innerText, Attr(attr));
			return ui;
		}

		public static Ui BeginButton(this Ui ui, Action<ButtonEvents, ButtonAttributes> events, object attr = null)
		{
			ui.NextId();
			var id = ui.NextIdHash;

			if (events != null)
			{
				var e = new ButtonEvents();
				var a = new ButtonAttributes();
				var d = new MouseEventData();

				events(e, a);

				if (ui.Events.Contains(MouseEvents.click, id))
					e.click?.Invoke(d);

				if (ui.Events.Contains(MouseEvents.mouseover, id))
					e.mouseover?.Invoke(d);

				if (ui.Events.Contains(MouseEvents.mouseout, id))
					e.mouseout?.Invoke(d);

				if (e.click != null)
					Scripts(ui).AppendEvent(MouseEvents.click, id);
				if (e.mouseover != null)
					Scripts(ui).AppendEvent(MouseEvents.mouseover, id);
				if (e.mouseout != null)
					Scripts(ui).AppendEvent(MouseEvents.mouseout, id);
			}

			// todo could be BeginButton if we could pass id with attr or attr didn't exist
			Dom(ui).Tab();
			Dom(ui).OpenBlock("button", id, attr == null ? null : Attr(attr));
			_indentLevel++;
			Elements.Push("button");
			return ui;
		}

		#endregion
	}
}