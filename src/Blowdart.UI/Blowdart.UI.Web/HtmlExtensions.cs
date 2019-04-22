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
            Dom(ui).Tab();
            Dom(ui).Append(text);
            return ui;
        }
        
        public static Ui Break(this Ui ui)
        {
            Dom(ui).Tab();
            Dom(ui).AppendTag("br");
            return ui;
        }

        public static Ui BeginElement(this Ui ui, string el, Attributes attr = null)
        {
            Dom(ui).Tab();
            Dom(ui).OpenBlock(el, attr);
            _indentLevel++;
            Elements.Push(el);
            return ui;
        }

        public static Ui Element(this Ui ui, string el, Attributes attr = null, Action action = null)
        {
            if (action == null)
            {
                ui.Element(el, null, attr);
                return ui;
            }

            Dom(ui).Tab();
            Dom(ui).OpenBlock(el, attr);
            _indentLevel++;
            Elements.Push(el);
            action();
            ui.EndElement(el);
            return ui;
        }

        public static Ui Element(this Ui ui, string el, object attr = null, Action action = null)
        {
            return ui.Element(el, Attr(attr), action);
        }

        public static Ui Element(this Ui ui, string el, Attributes attr = null, Action<Ui> action = null)
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

        public static Ui Element(this Ui ui, string el, string innerText, Attributes attr = null)
        {
            Dom(ui).Tab();
            Dom(ui).AppendTag(el, innerText, attr);
            return ui;
        }

        private static HtmlSystem Html(this Ui ui)
        {
            if (!(ui.System is HtmlSystem system))
                throw new NotSupportedException(ErrorStrings.MustUseHtmlSystem);
            return system;
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
            return Attributes.Attr(attr);
        }

        public static Attributes Attr(params object[] attr)
        {
            return Attributes.Attr(attr);
        }

        private static void Tab(this StringBuilder dom)
        {
            dom.Append(Environment.NewLine);
            for (var i = 0; i < _indentLevel; i++)
                dom.Append('\t');
        }

        #region Element Helpers

        #region form

        public static Ui BeginForm(this Ui ui, Attributes attr = null)
        {
            ui.BeginElement("form", attr);
            return ui;
        }
		
        public static Ui Form(this Ui ui, Attributes attr = null, Action action = null)
        {
            ui.Element("form", attr, action);
            return ui;
        }

        public static Ui Form(this Ui ui, object attr = null, Action action = null)
        {
            ui.Element("form", attr, action);
            return ui;
        }

        public static Ui Form(this Ui ui, Attributes attr, Action<Ui> action)
        {
            ui.Element("form", attr, action);
            return ui;
        }

        #endregion

        #region input

        public static Ui Input(this Ui ui, InputType inputType, Attributes attr = null)
        {
            var attribute = Attributes.Attr(new { type = inputType.ToString().ToLowerInvariant() });
            var attributes = attr == null ? attribute : Attributes.Attr(attribute, attr);
            ui.Element("input", null, attributes);
            return ui;
        }

        public static Ui Input(this Ui ui, InputType inputType, object attr = null)
        {
            var attributes = attr == null ? Attributes.Empty : Attributes.Attr(attr);
            return ui.Input(inputType, attributes);
        }

        #endregion

        #region Submit

        // Handled by the browser, so no routing information is needed
        public static Ui Submit(this Ui ui, string label = "Submit", object attr = null)
        {
            return ui.Input(InputType.Submit, Attr(new { value = label }));
        }

        //public static bool Submit(this Ui ui, string label, Action onSubmit)
        //{

        //}

        //public static Ui Submit(this Ui ui, InputType type, Attributes attr, Action onSubmit)
        //{
        //    if (onSubmit == null)
        //        return ui.Input(InputType.Submit, Attr(new { value = label ?? "Submit" }));
        //    if (!PreSubmit(ui, label, attr, out var template))
        //        return ui;
        //    onSubmit.Invoke();
        //    return ui;
        //}

        //public static Ui Submit<TService>(this Ui ui, string label, Action<dynamic> onSubmit)
        //{
        //    if (onSubmit == null)
        //        return ui.Input(InputType.Submit, Attr(new { value = label ?? "Submit" }));
        //    if (!PreSubmit(ui, label, attr, out var template))
        //        return ui;
        //    onSubmit(ui.Data.GetModel<TService>(template));
        //    return ui;
        //}

        //public static Ui Submit<TService, TModel>(this Ui ui, string label, Attributes attr = null, Action<TModel> onSubmit)
        //{
        //    if (onSubmit == null)
        //        return ui.Input(InputType.Submit, Attr(new { value = label ?? "Submit" }));
        //    if (!PreSubmit(ui, label, attr, out var template))
        //        return ui;
        //    onSubmit(ui.Data.GetModel<TService, TModel>(template));
        //    return ui;
        //}

        //private static bool PreSubmit(Ui ui, string label, Attributes attr, out string template)
        //{
        //    var value = label ?? "Submit";

        //    var inputAttr = Attr(new { type = "submit", value });
        //    var attributes = attr == null ? inputAttr : Attributes.Attr(attr, inputAttr);
        //    attributes.Inner.TryGetValue("action", out var action);
        //    template = action?.ToString() ?? string.Empty;

        //    var id = ui.NextIdHash;
        //    Dom(ui).AppendTag("input", id, null, attributes);
        //    Scripts(ui).AppendEvent("click", id);
        //    return !ui.Clicked.Contains(id);
        //}

        #endregion

        #endregion

        #region Usage Helpers

        public static Ui Div(this Ui ui, string @class, Action action)
        {
            ui.Div(Attributes.Attr(new { @class }), action);
            return ui;
        }

        public static Ui Div(this Ui ui, string @class, Action<Ui> action)
        {
            ui.Div(Attributes.Attr(new { @class }), action);
            return ui;
        }

        public static Ui Div(this Ui ui, string @class, Attributes attr, Action action)
        {
            ui.Div(Attributes.Attr(new { @class }, attr), action);
            return ui;
        }

        public static Ui Div(this Ui ui, string @class, Attributes attr, Action<Ui> action)
        {
            ui.Div(Attributes.Attr(new { @class }, attr), action);
            return ui;
        }

        public static Ui A(this Ui ui, string href, Action action = null)
        {
            ui.Div(Attributes.Attr(new { href }), action);
            return ui;
        }

        public static Ui A(this Ui ui, string href, Action<Ui> action)
        {
            ui.Div(Attributes.Attr(new { href }), action);
            return ui;
        }

        public static Ui A(this Ui ui, string href, string @class, Action action = null)
        {
            ui.A(Attributes.Attr(new { href, @class }), action);
            return ui;
        }

        public static Ui A(this Ui ui, string href, string @class, Action<Ui> action)
        {
            ui.A(Attributes.Attr(new { href, @class }), action);
            return ui;
        }

        public static Ui Img(this Ui ui, string src, Action action = null)
        {
            ui.Img(Attributes.Attr(new { src }), action);
            return ui;
        }

        public static Ui Img(this Ui ui, string src, Action<Ui> action)
        {
            ui.Img(Attributes.Attr(new { src }), action);
            return ui;
        }

        public static Ui Img(this Ui ui, string src, string @class, Action action = null)
        {
            ui.Img(Attributes.Attr(new { src, @class }), action);
            return ui;
        }

        public static Ui Img(this Ui ui, string src, string @class, Action<Ui> action)
        {
            ui.Img(Attributes.Attr(new { src, @class }), action);
            return ui;
        }

        #endregion
    }
}