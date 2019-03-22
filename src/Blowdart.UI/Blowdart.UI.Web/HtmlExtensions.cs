// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Blowdart.UI.Web.Internal;

namespace Blowdart.UI.Web
{
    public static class HtmlExtensions
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

        public static Attributes Attr(object attr)
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

        #region div

        public static Ui BeginDiv(this Ui ui, Attributes attr = null)
        {
            ui.BeginElement("div", attr);
            return ui;
        }

        public static Ui EndDiv(this Ui ui)
        {
            ui.EndElement("div");
            return ui;
        }

        public static Ui Div(this Ui ui, Attributes attr = null, Action action = null)
        {
            ui.Element("div", attr, action);
            return ui;
        }

        public static Ui Div(this Ui ui, Attributes attr, Action<Ui> action)
        {
            ui.Element("div", attr, action);
            return ui;
        }

        public static Ui Div(this Ui ui, string innerText, Attributes attr = null)
        {
            ui.Element("div", innerText, attr);
            return ui;
        }

        #endregion

        #region span

        public static Ui BeginSpan(this Ui ui, Attributes attr = null)
        {
            ui.BeginElement("span", attr);
            return ui;
        }

        public static Ui EndSpan(this Ui ui)
        {
            ui.EndElement("span");
            return ui;
        }

        public static Ui Span(this Ui ui, Attributes attr = null, Action action = null)
        {
            ui.Element("span", attr, action);
            return ui;
        }

        public static Ui Span(this Ui ui, Attributes attr = null, Action<Ui> action = null)
        {
            ui.Element("span", attr, action);
            return ui;
        }

        public static Ui Span(this Ui ui, string innerText, Attributes attr = null)
        {
            ui.Element("span", innerText, attr);
            return ui;
        }

        #endregion

        #region p 

        public static Ui BeginP(this Ui ui, Attributes attr = null)
        {
            ui.BeginElement("p", attr);
            return ui;
        }

        public static Ui EndP(this Ui ui)
        {
            ui.EndElement("p");
            return ui;
        }

        public static Ui P(this Ui ui, Attributes attr = null, Action action = null)
        {
            ui.Element("p", attr, action);
            return ui;
        }

        public static Ui P(this Ui ui, Attributes attr = null, Action<Ui> action = null)
        {
            ui.Element("p", attr, action);
            return ui;
        }

        public static Ui P(this Ui ui, string innerText, Attributes attr = null)
        {
            ui.Element("p", innerText, attr);
            return ui;
        }

        #endregion

        #region a 

        public static Ui BeginA(this Ui ui, Attributes attr = null)
        {
            ui.BeginElement("a", attr);
            return ui;
        }

        public static Ui EndA(this Ui ui)
        {
            ui.EndElement("a");
            return ui;
        }

        public static Ui A(this Ui ui, Attributes attr = null, Action action = null)
        {
            ui.Element("a", attr, action);
            return ui;
        }

        public static Ui A(this Ui ui, Attributes attr = null, Action<Ui> action = null)
        {
            ui.Element("a", attr, action);
            return ui;
        }

        public static Ui A(this Ui ui, string innerText, Attributes attr = null)
        {
            ui.Element("p", innerText, attr);
            return ui;
        }

        #endregion

        #region img 

        public static Ui BeginImg(this Ui ui, Attributes attr = null)
        {
            ui.BeginElement("img", attr);
            return ui;
        }

        public static Ui EndImg(this Ui ui)
        {
            ui.EndElement("img");
            return ui;
        }

        public static Ui Img(this Ui ui, Attributes attr = null, Action action = null)
        {
            ui.Element("img", attr, action);
            return ui;
        }

        public static Ui Img(this Ui ui, Attributes attr = null, Action<Ui> action = null)
        {
            ui.Element("img", attr, action);
            return ui;
        }

        public static Ui Img(this Ui ui, string innerText, Attributes attr = null)
        {
            ui.Element("img", innerText, attr);
            return ui;
        }

        #endregion

        #region h 

        public static Ui BeginH(this Ui ui, byte level, Attributes attr = null)
        {
            ui.BeginElement($"h{level}", attr);
            return ui;
        }

        public static Ui EndH(this Ui ui, byte level)
        {
            ui.EndElement($"h{level}");
            return ui;
        }

        public static Ui H(this Ui ui, byte level, Attributes attr = null, Action action = null)
        {
            ui.Element($"h{level}", attr, action);
            return ui;
        }

        public static Ui H(this Ui ui, byte level, Attributes attr = null, Action<Ui> action = null)
        {
            ui.Element($"h{level}", attr, action);
            return ui;
        }

        public static Ui H(this Ui ui, byte level, string innerText, Attributes attr = null)
        {
            ui.Element($"h{level}", innerText, attr);
            return ui;
        }

        #endregion

        #region pre

        public static Ui BeginPre(this Ui ui, Attributes attr = null)
        {
            ui.BeginElement("pre", attr);
            return ui;
        }

        public static Ui EndPre(this Ui ui)
        {
            ui.EndElement("pre");
            return ui;
        }

        public static Ui Pre(this Ui ui, Attributes attr = null, Action action = null)
        {
            ui.Element("pre", attr, action);
            return ui;
        }

        public static Ui Pre(this Ui ui, Attributes attr = null, Action<Ui> action = null)
        {
            ui.Element("pre", attr, action);
            return ui;
        }

        public static Ui Pre(this Ui ui, string innerText, Attributes attr = null)
        {
            ui.Element("pre", innerText, attr);
            return ui;
        }

        #endregion

        #region form

        public static Ui BeginForm(this Ui ui, Attributes attr = null)
        {
            ui.BeginElement("form", attr);
            return ui;
        }

        public static Ui EndForm(this Ui ui)
        {
            ui.EndElement("form");
            return ui;
        }

        public static Ui Form(this Ui ui, Attributes attr = null, Action action = null)
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

        #region 

        #region input

        public static Ui Input(this Ui ui, InputType type, Attributes attr = null)
        {
            ui.Element("input", null, Attributes.Attr(new { type }, attr));
            return ui;
        }

        #endregion

        #region Submit

        public static bool Submit(this Ui ui, string label = null, Attributes attr = null)
        {
            var id = ui.NextIdHash;
            Dom(ui).AppendTag("input", id, null, Attr(new { type = "submit", value = label ?? "Submit" }));
            Scripts(ui).AppendEvent("click", id);
            return ui.Clicked.Contains(id);
        }

        #endregion

        #endregion

        #endregion

        #region Pareto Helpers

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