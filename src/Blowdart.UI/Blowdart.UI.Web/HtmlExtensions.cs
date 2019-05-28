// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Blowdart.UI.Web.Internal;

namespace Blowdart.UI.Web
{
	public static partial class HtmlExtensions
    {
        private static byte _indentLevel;
        private static readonly Stack<string> Elements = new Stack<string>();

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
                throw new HtmlException($"Attempted to close a {el} without any open elements. You have a nesting issue somewhere.");
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

		#region DOM Events

		public static bool Click(this Ui ui, string el, string innerText, object attr = null)
        {
	        ui.NextId();
	        var id = ui.NextIdHash;
	        Dom(ui).AppendTag(el, id, innerText, attr == null ? null : Attr(attr));
	        Scripts(ui).AppendEvent(MouseEvents.click, id);
	        return ui.Clicked.Contains(id);
        }
		
		public static Ui Button(this Ui ui, string innerText, Action<ButtonEvents, ButtonAttributes> events,
			object attr = null)
		{
			ui.NextId();
			var id = ui.NextIdHash;

			if (events != null)
			{
				var e = new ButtonEvents();
				var a = new ButtonAttributes();
				var d = new MouseEventData();
				
				events(e, a);

				if (ui.Clicked.Contains(id))
					e.click?.Invoke(d);

				if (ui.MouseOver.Contains(id))
					e.mouseover?.Invoke(d);

				if (ui.MouseOut.Contains(id))
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

				if (ui.Clicked.Contains(id))
					e.click?.Invoke(d);

				if (ui.MouseOver.Contains(id))
					e.mouseover?.Invoke(d);

				if (ui.MouseOut.Contains(id))
					e.mouseout?.Invoke(d);
				
				if(e.click != null)
					Scripts(ui).AppendEvent(MouseEvents.click, id);
				if(e.mouseover != null)
					Scripts(ui).AppendEvent(MouseEvents.mouseover, id);
				if(e.mouseout != null)
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







		private static HtmlSystem Html(this Ui ui)
        {
            if (!(ui.System is HtmlSystem system))
                throw new NotSupportedException(ErrorStrings.MustUseHtmlSystem);
            return system;
        }

		private static StringBuilder Styles(Ui ui)
		{
			return ui.Html().Styles;
		}

		private static StringBuilder Dom(Ui ui)
        {
            return ui.Html().Dom;
        }

        private static StringBuilder Scripts(Ui ui)
        {
            return ui.Html().Scripts;
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
			var attribute = Attributes.Attr(new { type = inputType.ToString().ToLowerInvariant() });
			var attributes = attr == null ? attribute : Attributes.Attr(attribute, attr);
			ui.Element("input", null, attributes);
			return ui;
		}

		public static int Range(this Ui ui, int min, int max, int defaultValue, object attr = null)
		{
			var attribute = Attributes.Attr(new { type = "range" });
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
			return ui.Input(InputType.Submit, Attr(new { value = label }));
		}

		

		
	}

}