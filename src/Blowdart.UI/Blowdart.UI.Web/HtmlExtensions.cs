// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Blowdart.UI.Web.Internal;

namespace Blowdart.UI.Web
{
    public static class HtmlExtensions
    {
        private static byte tab;

        public static void BeginDiv(this Ui ui, object attr = null)
        {
            ui.BeginElement("div", attr);
        }

        public static void EndDiv(this Ui ui)
        {
            ui.EndElement("div");
        }

        public static void Div(this Ui ui, string innerText = null, object attr = null)
        {
            ui.Element("div", innerText, attr);
        }

        public static void Literal(this Ui ui, string text)
        {
            Dom(ui).Tab();
            Dom(ui).Append(text);
        }

        public static void BeginElement(this Ui ui, string el, object attr = null)
        {
            Dom(ui).Tab();
            Dom(ui).OpenBlock(el, attr);
            tab++;
        }

        public static void EndElement(this Ui ui, string el)
        {
            tab--;
            Dom(ui).Tab();
            Dom(ui).CloseBlock(el, true);
        }

        public static void Element(this Ui ui, string el, string innerText = null, object attr = null)
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
            for (var i = 0; i < tab; i++)
                dom.Append('\t');
        }
    }
}