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

        public static void Literal(this Ui ui, string text)
        {
            Dom(ui).Tab();
            Dom(ui).Append(text);
        }

        public static void BeginElement(this Ui ui, string el, Attributes attr = null)
        {
            Dom(ui).Tab();
            Dom(ui).OpenBlock(el, attr);
            _indentLevel++;
            Elements.Push(el);
        }

        public static void Element(this Ui ui, string el, Attributes attr = null, Action action = null)
        {
            if (action == null)
            {
                ui.Element(el, null, attr);
                return;
            }

            Dom(ui).Tab();
            Dom(ui).OpenBlock(el, attr);
            _indentLevel++;
            Elements.Push(el);
            action();
            ui.EndElement(el);
        }

        public static void EndElement(this Ui ui, string el)
        {
            if (Elements.Count == 0)
                throw new HtmlException($"Attempted to close a {el} without any open elements. You have a nesting issue somewhere.");
            if (el != Elements.Peek())
                throw new HtmlException($"Attempted to close a {el} before closing an inner {Elements.Peek()}");
            _indentLevel--;
            Dom(ui).Tab();
            Dom(ui).CloseBlock(el, true);
            Elements.Pop();
        }

        public static void Element(this Ui ui, string el, string innerText, Attributes attr = null)
        {
            Dom(ui).Tab();
            Dom(ui).AppendTag(el, innerText, attr);
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

        private static void Tab(this StringBuilder dom)
        {
            dom.Append(Environment.NewLine);
            for (var i = 0; i < _indentLevel; i++)
                dom.Append('\t');
        }

        #region Element Helpers

        #region div

        public static void BeginDiv(this Ui ui, Attributes attr = null)
        {
            ui.BeginElement("div", attr);
        }

        public static void EndDiv(this Ui ui)
        {
            ui.EndElement("div");
        }

        public static void Div(this Ui ui, Attributes attr = null, Action action = null)
        {
            ui.Element("div", attr, action);
        }

        public static void Div(this Ui ui, string innerText, Attributes attr = null)
        {
            ui.Element("div", innerText, attr);
        }

        #endregion

        #region span

        public static void BeginSpan(this Ui ui, Attributes attr = null)
        {
            ui.BeginElement("span", attr);
        }

        public static void EndSpan(this Ui ui)
        {
            ui.EndElement("span");
        }

        public static void Span(this Ui ui, Attributes attr = null, Action action = null)
        {
            ui.Element("span", attr, action);
        }

        public static void Span(this Ui ui, string innerText, Attributes attr = null)
        {
            ui.Element("span", innerText, attr);
        }

        #endregion

        #region p 

        public static void BeginP(this Ui ui, Attributes attr = null)
        {
            ui.BeginElement("p", attr);
        }

        public static void EndP(this Ui ui)
        {
            ui.EndElement("p");
        }

        public static void P(this Ui ui, Attributes attr = null, Action action = null)
        {
            ui.Element("p", attr, action);
        }

        public static void P(this Ui ui, string innerText, Attributes attr = null)
        {
            ui.Element("p", innerText, attr);
        }

        #endregion

        #region a 

        public static void BeginA(this Ui ui, Attributes attr = null)
        {
            ui.BeginElement("a", attr);
        }

        public static void EndA(this Ui ui)
        {
            ui.EndElement("a");
        }

        public static void A(this Ui ui, Attributes attr = null, Action action = null)
        {
            ui.Element("a", attr, action);
        }

        public static void A(this Ui ui, string innerText, Attributes attr = null)
        {
            ui.Element("p", innerText, attr);
        }

        #endregion

        #region img 

        public static void BeginImg(this Ui ui, Attributes attr = null)
        {
            ui.BeginElement("img", attr);
        }

        public static void EndImg(this Ui ui)
        {
            ui.EndElement("img");
        }

        public static void Img(this Ui ui, Attributes attr = null, Action action = null)
        {
            ui.Element("img", attr, action);
        }

        public static void Img(this Ui ui, string innerText, Attributes attr = null)
        {
            ui.Element("img", innerText, attr);
        }

        #endregion

        #region h 

        public static void BeginH(this Ui ui, byte level, Attributes attr = null)
        {
            ui.BeginElement($"h{level}", attr);
        }

        public static void EndH(this Ui ui, byte level)
        {
            ui.EndElement($"h{level}");
        }

        public static void H(this Ui ui, byte level, Attributes attr = null, Action action = null)
        {
            ui.Element($"h{level}", attr, action);
        }

        public static void H(this Ui ui, byte level, string innerText, Attributes attr = null)
        {
            ui.Element($"h{level}", innerText, attr);
        }

        #endregion

        #region pre

        public static void BeginPre(this Ui ui, Attributes attr = null)
        {
            ui.BeginElement("pre", attr);
        }

        public static void EndPre(this Ui ui)
        {
            ui.EndElement("pre");
        }

        public static void Pre(this Ui ui, Attributes attr = null, Action action = null)
        {
            ui.Element("pre", attr, action);
        }

        public static void Pre(this Ui ui, string innerText, Attributes attr = null)
        {
            ui.Element("pre", innerText, attr);
        }

        #endregion

        #endregion

        #region Pareto Helpers

        public static void Div(this Ui ui, string @class, Action action)
        {
            ui.Div(Attributes.Attr(new { @class }), action);
        }

        public static void Div(this Ui ui, string @class, Attributes attr, Action action)
        {
            ui.Div(Attributes.Attr(new { @class }, attr), action);
        }

        public static void A(this Ui ui, string href, Action action = null)
        {
            ui.Div(Attributes.Attr(new { href }), action);
        }

        public static void A(this Ui ui, string href, string @class, Action action = null)
        {
            ui.A(Attributes.Attr(new { href, @class }), action);
        }

        public static void Img(this Ui ui, string src, Action action = null)
        {
            ui.Img(Attributes.Attr(new { src }), action);
        }

        public static void Img(this Ui ui, string src, string @class, Action action = null)
        {
            ui.Img(Attributes.Attr(new { src, @class }), action);
        }

        #endregion
    }
}