
// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Blowdart.UI.Web
{

	public static partial class HtmlExtensions
	{

		public static Ui BeginAddress(this Ui ui, object attr = null)
		{
			return ui.BeginElement("address", attr != null ? Attr(attr) : null);
		}

		public static Ui EndAddress(this Ui ui)
		{
			return ui.EndElement("address");
		}

		public static Ui BeginArticle(this Ui ui, object attr = null)
		{
			return ui.BeginElement("article", attr != null ? Attr(attr) : null);
		}

		public static Ui EndArticle(this Ui ui)
		{
			return ui.EndElement("article");
		}

		public static Ui BeginAside(this Ui ui, object attr = null)
		{
			return ui.BeginElement("aside", attr != null ? Attr(attr) : null);
		}

		public static Ui EndAside(this Ui ui)
		{
			return ui.EndElement("aside");
		}

		public static Ui BeginBlockquote(this Ui ui, object attr = null)
		{
			return ui.BeginElement("blockquote", attr != null ? Attr(attr) : null);
		}

		public static Ui EndBlockquote(this Ui ui)
		{
			return ui.EndElement("blockquote");
		}

		public static Ui BeginCanvas(this Ui ui, object attr = null)
		{
			return ui.BeginElement("canvas", attr != null ? Attr(attr) : null);
		}

		public static Ui EndCanvas(this Ui ui)
		{
			return ui.EndElement("canvas");
		}

		public static Ui BeginDd(this Ui ui, object attr = null)
		{
			return ui.BeginElement("dd", attr != null ? Attr(attr) : null);
		}

		public static Ui EndDd(this Ui ui)
		{
			return ui.EndElement("dd");
		}

		public static Ui BeginDiv(this Ui ui, object attr = null)
		{
			return ui.BeginElement("div", attr != null ? Attr(attr) : null);
		}

		public static Ui EndDiv(this Ui ui)
		{
			return ui.EndElement("div");
		}

		public static Ui BeginDl(this Ui ui, object attr = null)
		{
			return ui.BeginElement("dl", attr != null ? Attr(attr) : null);
		}

		public static Ui EndDl(this Ui ui)
		{
			return ui.EndElement("dl");
		}

		public static Ui BeginDt(this Ui ui, object attr = null)
		{
			return ui.BeginElement("dt", attr != null ? Attr(attr) : null);
		}

		public static Ui EndDt(this Ui ui)
		{
			return ui.EndElement("dt");
		}

		public static Ui BeginFieldset(this Ui ui, object attr = null)
		{
			return ui.BeginElement("fieldset", attr != null ? Attr(attr) : null);
		}

		public static Ui EndFieldset(this Ui ui)
		{
			return ui.EndElement("fieldset");
		}

		public static Ui BeginFigcaption(this Ui ui, object attr = null)
		{
			return ui.BeginElement("figcaption", attr != null ? Attr(attr) : null);
		}

		public static Ui EndFigcaption(this Ui ui)
		{
			return ui.EndElement("figcaption");
		}

		public static Ui BeginFigure(this Ui ui, object attr = null)
		{
			return ui.BeginElement("figure", attr != null ? Attr(attr) : null);
		}

		public static Ui EndFigure(this Ui ui)
		{
			return ui.EndElement("figure");
		}

		public static Ui BeginFooter(this Ui ui, object attr = null)
		{
			return ui.BeginElement("footer", attr != null ? Attr(attr) : null);
		}

		public static Ui EndFooter(this Ui ui)
		{
			return ui.EndElement("footer");
		}

		public static Ui BeginForm(this Ui ui, object attr = null)
		{
			return ui.BeginElement("form", attr != null ? Attr(attr) : null);
		}

		public static Ui EndForm(this Ui ui)
		{
			return ui.EndElement("form");
		}

		public static Ui BeginH1(this Ui ui, object attr = null)
		{
			return ui.BeginElement("h1", attr != null ? Attr(attr) : null);
		}

		public static Ui EndH1(this Ui ui)
		{
			return ui.EndElement("h1");
		}

		public static Ui BeginH2(this Ui ui, object attr = null)
		{
			return ui.BeginElement("h2", attr != null ? Attr(attr) : null);
		}

		public static Ui EndH2(this Ui ui)
		{
			return ui.EndElement("h2");
		}

		public static Ui BeginH3(this Ui ui, object attr = null)
		{
			return ui.BeginElement("h3", attr != null ? Attr(attr) : null);
		}

		public static Ui EndH3(this Ui ui)
		{
			return ui.EndElement("h3");
		}

		public static Ui BeginH4(this Ui ui, object attr = null)
		{
			return ui.BeginElement("h4", attr != null ? Attr(attr) : null);
		}

		public static Ui EndH4(this Ui ui)
		{
			return ui.EndElement("h4");
		}

		public static Ui BeginH5(this Ui ui, object attr = null)
		{
			return ui.BeginElement("h5", attr != null ? Attr(attr) : null);
		}

		public static Ui EndH5(this Ui ui)
		{
			return ui.EndElement("h5");
		}

		public static Ui BeginH6(this Ui ui, object attr = null)
		{
			return ui.BeginElement("h6", attr != null ? Attr(attr) : null);
		}

		public static Ui EndH6(this Ui ui)
		{
			return ui.EndElement("h6");
		}

		public static Ui BeginHeader(this Ui ui, object attr = null)
		{
			return ui.BeginElement("header", attr != null ? Attr(attr) : null);
		}

		public static Ui EndHeader(this Ui ui)
		{
			return ui.EndElement("header");
		}

		public static Ui BeginHr(this Ui ui, object attr = null)
		{
			return ui.BeginElement("hr", attr != null ? Attr(attr) : null);
		}

		public static Ui EndHr(this Ui ui)
		{
			return ui.EndElement("hr");
		}

		public static Ui BeginLi(this Ui ui, object attr = null)
		{
			return ui.BeginElement("li", attr != null ? Attr(attr) : null);
		}

		public static Ui EndLi(this Ui ui)
		{
			return ui.EndElement("li");
		}

		public static Ui BeginMain(this Ui ui, object attr = null)
		{
			return ui.BeginElement("main", attr != null ? Attr(attr) : null);
		}

		public static Ui EndMain(this Ui ui)
		{
			return ui.EndElement("main");
		}

		public static Ui BeginNav(this Ui ui, object attr = null)
		{
			return ui.BeginElement("nav", attr != null ? Attr(attr) : null);
		}

		public static Ui EndNav(this Ui ui)
		{
			return ui.EndElement("nav");
		}

		public static Ui BeginNoscript(this Ui ui, object attr = null)
		{
			return ui.BeginElement("noscript", attr != null ? Attr(attr) : null);
		}

		public static Ui EndNoscript(this Ui ui)
		{
			return ui.EndElement("noscript");
		}

		public static Ui BeginOl(this Ui ui, object attr = null)
		{
			return ui.BeginElement("ol", attr != null ? Attr(attr) : null);
		}

		public static Ui EndOl(this Ui ui)
		{
			return ui.EndElement("ol");
		}

		public static Ui BeginP(this Ui ui, object attr = null)
		{
			return ui.BeginElement("p", attr != null ? Attr(attr) : null);
		}

		public static Ui EndP(this Ui ui)
		{
			return ui.EndElement("p");
		}

		public static Ui BeginPre(this Ui ui, object attr = null)
		{
			return ui.BeginElement("pre", attr != null ? Attr(attr) : null);
		}

		public static Ui EndPre(this Ui ui)
		{
			return ui.EndElement("pre");
		}

		public static Ui BeginSection(this Ui ui, object attr = null)
		{
			return ui.BeginElement("section", attr != null ? Attr(attr) : null);
		}

		public static Ui EndSection(this Ui ui)
		{
			return ui.EndElement("section");
		}

		public static Ui BeginTable(this Ui ui, object attr = null)
		{
			return ui.BeginElement("table", attr != null ? Attr(attr) : null);
		}

		public static Ui EndTable(this Ui ui)
		{
			return ui.EndElement("table");
		}

		public static Ui BeginTfoot(this Ui ui, object attr = null)
		{
			return ui.BeginElement("tfoot", attr != null ? Attr(attr) : null);
		}

		public static Ui EndTfoot(this Ui ui)
		{
			return ui.EndElement("tfoot");
		}

		public static Ui BeginUl(this Ui ui, object attr = null)
		{
			return ui.BeginElement("ul", attr != null ? Attr(attr) : null);
		}

		public static Ui EndUl(this Ui ui)
		{
			return ui.EndElement("ul");
		}

		public static Ui BeginVideo(this Ui ui, object attr = null)
		{
			return ui.BeginElement("video", attr != null ? Attr(attr) : null);
		}

		public static Ui EndVideo(this Ui ui)
		{
			return ui.EndElement("video");
		}
	}
}

