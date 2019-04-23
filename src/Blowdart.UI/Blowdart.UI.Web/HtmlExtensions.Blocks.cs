
// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Blowdart.UI.Web
{

	public static partial class HtmlExtensions
	{

		#region address

		public static Ui BeginAddress(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("address", attr);
		}

		public static Ui EndAddress(this Ui ui)
		{
			return ui.EndElement("address");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginAddress();
		///		action();
		///		ui.EndAddress();
		///	</code>
		/// </summary>
		public static Ui Address(this Ui ui, Action action)
		{
			return ui.Element("address", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginAddress(attr);
		///		action();
		///		ui.EndAddress();
		///	</code>
		/// </summary>
		public static Ui Address(this Ui ui, object attr, Action action)
		{
			return ui.Element("address", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginAddress();
		///		action(ui);
		///		ui.EndAddress();
		///	</code>
		/// </summary>
		public static Ui Address(this Ui ui, Action<Ui> action)
		{
			return ui.Element("address", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginAddress(attr);
		///		action(ui);
		///		ui.EndAddress();
		///	</code>
		/// </summary>
		public static Ui Address(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("address", attr, action);
		}

		#endregion

		#region article

		public static Ui BeginArticle(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("article", attr);
		}

		public static Ui EndArticle(this Ui ui)
		{
			return ui.EndElement("article");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginArticle();
		///		action();
		///		ui.EndArticle();
		///	</code>
		/// </summary>
		public static Ui Article(this Ui ui, Action action)
		{
			return ui.Element("article", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginArticle(attr);
		///		action();
		///		ui.EndArticle();
		///	</code>
		/// </summary>
		public static Ui Article(this Ui ui, object attr, Action action)
		{
			return ui.Element("article", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginArticle();
		///		action(ui);
		///		ui.EndArticle();
		///	</code>
		/// </summary>
		public static Ui Article(this Ui ui, Action<Ui> action)
		{
			return ui.Element("article", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginArticle(attr);
		///		action(ui);
		///		ui.EndArticle();
		///	</code>
		/// </summary>
		public static Ui Article(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("article", attr, action);
		}

		#endregion

		#region aside

		public static Ui BeginAside(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("aside", attr);
		}

		public static Ui EndAside(this Ui ui)
		{
			return ui.EndElement("aside");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginAside();
		///		action();
		///		ui.EndAside();
		///	</code>
		/// </summary>
		public static Ui Aside(this Ui ui, Action action)
		{
			return ui.Element("aside", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginAside(attr);
		///		action();
		///		ui.EndAside();
		///	</code>
		/// </summary>
		public static Ui Aside(this Ui ui, object attr, Action action)
		{
			return ui.Element("aside", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginAside();
		///		action(ui);
		///		ui.EndAside();
		///	</code>
		/// </summary>
		public static Ui Aside(this Ui ui, Action<Ui> action)
		{
			return ui.Element("aside", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginAside(attr);
		///		action(ui);
		///		ui.EndAside();
		///	</code>
		/// </summary>
		public static Ui Aside(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("aside", attr, action);
		}

		#endregion

		#region blockquote

		public static Ui BeginBlockquote(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("blockquote", attr);
		}

		public static Ui EndBlockquote(this Ui ui)
		{
			return ui.EndElement("blockquote");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginBlockquote();
		///		action();
		///		ui.EndBlockquote();
		///	</code>
		/// </summary>
		public static Ui Blockquote(this Ui ui, Action action)
		{
			return ui.Element("blockquote", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginBlockquote(attr);
		///		action();
		///		ui.EndBlockquote();
		///	</code>
		/// </summary>
		public static Ui Blockquote(this Ui ui, object attr, Action action)
		{
			return ui.Element("blockquote", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginBlockquote();
		///		action(ui);
		///		ui.EndBlockquote();
		///	</code>
		/// </summary>
		public static Ui Blockquote(this Ui ui, Action<Ui> action)
		{
			return ui.Element("blockquote", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginBlockquote(attr);
		///		action(ui);
		///		ui.EndBlockquote();
		///	</code>
		/// </summary>
		public static Ui Blockquote(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("blockquote", attr, action);
		}

		#endregion

		#region canvas

		public static Ui BeginCanvas(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("canvas", attr);
		}

		public static Ui EndCanvas(this Ui ui)
		{
			return ui.EndElement("canvas");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginCanvas();
		///		action();
		///		ui.EndCanvas();
		///	</code>
		/// </summary>
		public static Ui Canvas(this Ui ui, Action action)
		{
			return ui.Element("canvas", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginCanvas(attr);
		///		action();
		///		ui.EndCanvas();
		///	</code>
		/// </summary>
		public static Ui Canvas(this Ui ui, object attr, Action action)
		{
			return ui.Element("canvas", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginCanvas();
		///		action(ui);
		///		ui.EndCanvas();
		///	</code>
		/// </summary>
		public static Ui Canvas(this Ui ui, Action<Ui> action)
		{
			return ui.Element("canvas", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginCanvas(attr);
		///		action(ui);
		///		ui.EndCanvas();
		///	</code>
		/// </summary>
		public static Ui Canvas(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("canvas", attr, action);
		}

		#endregion

		#region dd

		public static Ui BeginDd(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("dd", attr);
		}

		public static Ui EndDd(this Ui ui)
		{
			return ui.EndElement("dd");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDd();
		///		action();
		///		ui.EndDd();
		///	</code>
		/// </summary>
		public static Ui Dd(this Ui ui, Action action)
		{
			return ui.Element("dd", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDd(attr);
		///		action();
		///		ui.EndDd();
		///	</code>
		/// </summary>
		public static Ui Dd(this Ui ui, object attr, Action action)
		{
			return ui.Element("dd", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDd();
		///		action(ui);
		///		ui.EndDd();
		///	</code>
		/// </summary>
		public static Ui Dd(this Ui ui, Action<Ui> action)
		{
			return ui.Element("dd", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDd(attr);
		///		action(ui);
		///		ui.EndDd();
		///	</code>
		/// </summary>
		public static Ui Dd(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("dd", attr, action);
		}

		#endregion

		#region div

		public static Ui BeginDiv(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("div", attr);
		}

		public static Ui EndDiv(this Ui ui)
		{
			return ui.EndElement("div");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDiv();
		///		action();
		///		ui.EndDiv();
		///	</code>
		/// </summary>
		public static Ui Div(this Ui ui, Action action)
		{
			return ui.Element("div", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDiv(attr);
		///		action();
		///		ui.EndDiv();
		///	</code>
		/// </summary>
		public static Ui Div(this Ui ui, object attr, Action action)
		{
			return ui.Element("div", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDiv();
		///		action(ui);
		///		ui.EndDiv();
		///	</code>
		/// </summary>
		public static Ui Div(this Ui ui, Action<Ui> action)
		{
			return ui.Element("div", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDiv(attr);
		///		action(ui);
		///		ui.EndDiv();
		///	</code>
		/// </summary>
		public static Ui Div(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("div", attr, action);
		}

		#endregion

		#region dl

		public static Ui BeginDl(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("dl", attr);
		}

		public static Ui EndDl(this Ui ui)
		{
			return ui.EndElement("dl");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDl();
		///		action();
		///		ui.EndDl();
		///	</code>
		/// </summary>
		public static Ui Dl(this Ui ui, Action action)
		{
			return ui.Element("dl", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDl(attr);
		///		action();
		///		ui.EndDl();
		///	</code>
		/// </summary>
		public static Ui Dl(this Ui ui, object attr, Action action)
		{
			return ui.Element("dl", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDl();
		///		action(ui);
		///		ui.EndDl();
		///	</code>
		/// </summary>
		public static Ui Dl(this Ui ui, Action<Ui> action)
		{
			return ui.Element("dl", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDl(attr);
		///		action(ui);
		///		ui.EndDl();
		///	</code>
		/// </summary>
		public static Ui Dl(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("dl", attr, action);
		}

		#endregion

		#region dt

		public static Ui BeginDt(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("dt", attr);
		}

		public static Ui EndDt(this Ui ui)
		{
			return ui.EndElement("dt");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDt();
		///		action();
		///		ui.EndDt();
		///	</code>
		/// </summary>
		public static Ui Dt(this Ui ui, Action action)
		{
			return ui.Element("dt", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDt(attr);
		///		action();
		///		ui.EndDt();
		///	</code>
		/// </summary>
		public static Ui Dt(this Ui ui, object attr, Action action)
		{
			return ui.Element("dt", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDt();
		///		action(ui);
		///		ui.EndDt();
		///	</code>
		/// </summary>
		public static Ui Dt(this Ui ui, Action<Ui> action)
		{
			return ui.Element("dt", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginDt(attr);
		///		action(ui);
		///		ui.EndDt();
		///	</code>
		/// </summary>
		public static Ui Dt(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("dt", attr, action);
		}

		#endregion

		#region fieldset

		public static Ui BeginFieldset(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("fieldset", attr);
		}

		public static Ui EndFieldset(this Ui ui)
		{
			return ui.EndElement("fieldset");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFieldset();
		///		action();
		///		ui.EndFieldset();
		///	</code>
		/// </summary>
		public static Ui Fieldset(this Ui ui, Action action)
		{
			return ui.Element("fieldset", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFieldset(attr);
		///		action();
		///		ui.EndFieldset();
		///	</code>
		/// </summary>
		public static Ui Fieldset(this Ui ui, object attr, Action action)
		{
			return ui.Element("fieldset", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFieldset();
		///		action(ui);
		///		ui.EndFieldset();
		///	</code>
		/// </summary>
		public static Ui Fieldset(this Ui ui, Action<Ui> action)
		{
			return ui.Element("fieldset", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFieldset(attr);
		///		action(ui);
		///		ui.EndFieldset();
		///	</code>
		/// </summary>
		public static Ui Fieldset(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("fieldset", attr, action);
		}

		#endregion

		#region figcaption

		public static Ui BeginFigcaption(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("figcaption", attr);
		}

		public static Ui EndFigcaption(this Ui ui)
		{
			return ui.EndElement("figcaption");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFigcaption();
		///		action();
		///		ui.EndFigcaption();
		///	</code>
		/// </summary>
		public static Ui Figcaption(this Ui ui, Action action)
		{
			return ui.Element("figcaption", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFigcaption(attr);
		///		action();
		///		ui.EndFigcaption();
		///	</code>
		/// </summary>
		public static Ui Figcaption(this Ui ui, object attr, Action action)
		{
			return ui.Element("figcaption", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFigcaption();
		///		action(ui);
		///		ui.EndFigcaption();
		///	</code>
		/// </summary>
		public static Ui Figcaption(this Ui ui, Action<Ui> action)
		{
			return ui.Element("figcaption", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFigcaption(attr);
		///		action(ui);
		///		ui.EndFigcaption();
		///	</code>
		/// </summary>
		public static Ui Figcaption(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("figcaption", attr, action);
		}

		#endregion

		#region figure

		public static Ui BeginFigure(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("figure", attr);
		}

		public static Ui EndFigure(this Ui ui)
		{
			return ui.EndElement("figure");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFigure();
		///		action();
		///		ui.EndFigure();
		///	</code>
		/// </summary>
		public static Ui Figure(this Ui ui, Action action)
		{
			return ui.Element("figure", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFigure(attr);
		///		action();
		///		ui.EndFigure();
		///	</code>
		/// </summary>
		public static Ui Figure(this Ui ui, object attr, Action action)
		{
			return ui.Element("figure", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFigure();
		///		action(ui);
		///		ui.EndFigure();
		///	</code>
		/// </summary>
		public static Ui Figure(this Ui ui, Action<Ui> action)
		{
			return ui.Element("figure", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFigure(attr);
		///		action(ui);
		///		ui.EndFigure();
		///	</code>
		/// </summary>
		public static Ui Figure(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("figure", attr, action);
		}

		#endregion

		#region footer

		public static Ui BeginFooter(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("footer", attr);
		}

		public static Ui EndFooter(this Ui ui)
		{
			return ui.EndElement("footer");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFooter();
		///		action();
		///		ui.EndFooter();
		///	</code>
		/// </summary>
		public static Ui Footer(this Ui ui, Action action)
		{
			return ui.Element("footer", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFooter(attr);
		///		action();
		///		ui.EndFooter();
		///	</code>
		/// </summary>
		public static Ui Footer(this Ui ui, object attr, Action action)
		{
			return ui.Element("footer", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFooter();
		///		action(ui);
		///		ui.EndFooter();
		///	</code>
		/// </summary>
		public static Ui Footer(this Ui ui, Action<Ui> action)
		{
			return ui.Element("footer", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginFooter(attr);
		///		action(ui);
		///		ui.EndFooter();
		///	</code>
		/// </summary>
		public static Ui Footer(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("footer", attr, action);
		}

		#endregion

		#region form

		public static Ui BeginForm(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("form", attr);
		}

		public static Ui EndForm(this Ui ui)
		{
			return ui.EndElement("form");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginForm();
		///		action();
		///		ui.EndForm();
		///	</code>
		/// </summary>
		public static Ui Form(this Ui ui, Action action)
		{
			return ui.Element("form", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginForm(attr);
		///		action();
		///		ui.EndForm();
		///	</code>
		/// </summary>
		public static Ui Form(this Ui ui, object attr, Action action)
		{
			return ui.Element("form", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginForm();
		///		action(ui);
		///		ui.EndForm();
		///	</code>
		/// </summary>
		public static Ui Form(this Ui ui, Action<Ui> action)
		{
			return ui.Element("form", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginForm(attr);
		///		action(ui);
		///		ui.EndForm();
		///	</code>
		/// </summary>
		public static Ui Form(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("form", attr, action);
		}

		#endregion

		#region h1

		public static Ui BeginH1(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("h1", attr);
		}

		public static Ui EndH1(this Ui ui)
		{
			return ui.EndElement("h1");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH1();
		///		action();
		///		ui.EndH1();
		///	</code>
		/// </summary>
		public static Ui H1(this Ui ui, Action action)
		{
			return ui.Element("h1", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH1(attr);
		///		action();
		///		ui.EndH1();
		///	</code>
		/// </summary>
		public static Ui H1(this Ui ui, object attr, Action action)
		{
			return ui.Element("h1", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH1();
		///		action(ui);
		///		ui.EndH1();
		///	</code>
		/// </summary>
		public static Ui H1(this Ui ui, Action<Ui> action)
		{
			return ui.Element("h1", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH1(attr);
		///		action(ui);
		///		ui.EndH1();
		///	</code>
		/// </summary>
		public static Ui H1(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("h1", attr, action);
		}

		#endregion

		#region h2

		public static Ui BeginH2(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("h2", attr);
		}

		public static Ui EndH2(this Ui ui)
		{
			return ui.EndElement("h2");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH2();
		///		action();
		///		ui.EndH2();
		///	</code>
		/// </summary>
		public static Ui H2(this Ui ui, Action action)
		{
			return ui.Element("h2", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH2(attr);
		///		action();
		///		ui.EndH2();
		///	</code>
		/// </summary>
		public static Ui H2(this Ui ui, object attr, Action action)
		{
			return ui.Element("h2", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH2();
		///		action(ui);
		///		ui.EndH2();
		///	</code>
		/// </summary>
		public static Ui H2(this Ui ui, Action<Ui> action)
		{
			return ui.Element("h2", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH2(attr);
		///		action(ui);
		///		ui.EndH2();
		///	</code>
		/// </summary>
		public static Ui H2(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("h2", attr, action);
		}

		#endregion

		#region h3

		public static Ui BeginH3(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("h3", attr);
		}

		public static Ui EndH3(this Ui ui)
		{
			return ui.EndElement("h3");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH3();
		///		action();
		///		ui.EndH3();
		///	</code>
		/// </summary>
		public static Ui H3(this Ui ui, Action action)
		{
			return ui.Element("h3", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH3(attr);
		///		action();
		///		ui.EndH3();
		///	</code>
		/// </summary>
		public static Ui H3(this Ui ui, object attr, Action action)
		{
			return ui.Element("h3", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH3();
		///		action(ui);
		///		ui.EndH3();
		///	</code>
		/// </summary>
		public static Ui H3(this Ui ui, Action<Ui> action)
		{
			return ui.Element("h3", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH3(attr);
		///		action(ui);
		///		ui.EndH3();
		///	</code>
		/// </summary>
		public static Ui H3(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("h3", attr, action);
		}

		#endregion

		#region h4

		public static Ui BeginH4(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("h4", attr);
		}

		public static Ui EndH4(this Ui ui)
		{
			return ui.EndElement("h4");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH4();
		///		action();
		///		ui.EndH4();
		///	</code>
		/// </summary>
		public static Ui H4(this Ui ui, Action action)
		{
			return ui.Element("h4", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH4(attr);
		///		action();
		///		ui.EndH4();
		///	</code>
		/// </summary>
		public static Ui H4(this Ui ui, object attr, Action action)
		{
			return ui.Element("h4", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH4();
		///		action(ui);
		///		ui.EndH4();
		///	</code>
		/// </summary>
		public static Ui H4(this Ui ui, Action<Ui> action)
		{
			return ui.Element("h4", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH4(attr);
		///		action(ui);
		///		ui.EndH4();
		///	</code>
		/// </summary>
		public static Ui H4(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("h4", attr, action);
		}

		#endregion

		#region h5

		public static Ui BeginH5(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("h5", attr);
		}

		public static Ui EndH5(this Ui ui)
		{
			return ui.EndElement("h5");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH5();
		///		action();
		///		ui.EndH5();
		///	</code>
		/// </summary>
		public static Ui H5(this Ui ui, Action action)
		{
			return ui.Element("h5", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH5(attr);
		///		action();
		///		ui.EndH5();
		///	</code>
		/// </summary>
		public static Ui H5(this Ui ui, object attr, Action action)
		{
			return ui.Element("h5", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH5();
		///		action(ui);
		///		ui.EndH5();
		///	</code>
		/// </summary>
		public static Ui H5(this Ui ui, Action<Ui> action)
		{
			return ui.Element("h5", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH5(attr);
		///		action(ui);
		///		ui.EndH5();
		///	</code>
		/// </summary>
		public static Ui H5(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("h5", attr, action);
		}

		#endregion

		#region h6

		public static Ui BeginH6(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("h6", attr);
		}

		public static Ui EndH6(this Ui ui)
		{
			return ui.EndElement("h6");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH6();
		///		action();
		///		ui.EndH6();
		///	</code>
		/// </summary>
		public static Ui H6(this Ui ui, Action action)
		{
			return ui.Element("h6", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH6(attr);
		///		action();
		///		ui.EndH6();
		///	</code>
		/// </summary>
		public static Ui H6(this Ui ui, object attr, Action action)
		{
			return ui.Element("h6", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH6();
		///		action(ui);
		///		ui.EndH6();
		///	</code>
		/// </summary>
		public static Ui H6(this Ui ui, Action<Ui> action)
		{
			return ui.Element("h6", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginH6(attr);
		///		action(ui);
		///		ui.EndH6();
		///	</code>
		/// </summary>
		public static Ui H6(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("h6", attr, action);
		}

		#endregion

		#region header

		public static Ui BeginHeader(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("header", attr);
		}

		public static Ui EndHeader(this Ui ui)
		{
			return ui.EndElement("header");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginHeader();
		///		action();
		///		ui.EndHeader();
		///	</code>
		/// </summary>
		public static Ui Header(this Ui ui, Action action)
		{
			return ui.Element("header", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginHeader(attr);
		///		action();
		///		ui.EndHeader();
		///	</code>
		/// </summary>
		public static Ui Header(this Ui ui, object attr, Action action)
		{
			return ui.Element("header", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginHeader();
		///		action(ui);
		///		ui.EndHeader();
		///	</code>
		/// </summary>
		public static Ui Header(this Ui ui, Action<Ui> action)
		{
			return ui.Element("header", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginHeader(attr);
		///		action(ui);
		///		ui.EndHeader();
		///	</code>
		/// </summary>
		public static Ui Header(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("header", attr, action);
		}

		#endregion

		#region hr

		public static Ui BeginHr(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("hr", attr);
		}

		public static Ui EndHr(this Ui ui)
		{
			return ui.EndElement("hr");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginHr();
		///		action();
		///		ui.EndHr();
		///	</code>
		/// </summary>
		public static Ui Hr(this Ui ui, Action action)
		{
			return ui.Element("hr", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginHr(attr);
		///		action();
		///		ui.EndHr();
		///	</code>
		/// </summary>
		public static Ui Hr(this Ui ui, object attr, Action action)
		{
			return ui.Element("hr", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginHr();
		///		action(ui);
		///		ui.EndHr();
		///	</code>
		/// </summary>
		public static Ui Hr(this Ui ui, Action<Ui> action)
		{
			return ui.Element("hr", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginHr(attr);
		///		action(ui);
		///		ui.EndHr();
		///	</code>
		/// </summary>
		public static Ui Hr(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("hr", attr, action);
		}

		#endregion

		#region li

		public static Ui BeginLi(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("li", attr);
		}

		public static Ui EndLi(this Ui ui)
		{
			return ui.EndElement("li");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginLi();
		///		action();
		///		ui.EndLi();
		///	</code>
		/// </summary>
		public static Ui Li(this Ui ui, Action action)
		{
			return ui.Element("li", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginLi(attr);
		///		action();
		///		ui.EndLi();
		///	</code>
		/// </summary>
		public static Ui Li(this Ui ui, object attr, Action action)
		{
			return ui.Element("li", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginLi();
		///		action(ui);
		///		ui.EndLi();
		///	</code>
		/// </summary>
		public static Ui Li(this Ui ui, Action<Ui> action)
		{
			return ui.Element("li", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginLi(attr);
		///		action(ui);
		///		ui.EndLi();
		///	</code>
		/// </summary>
		public static Ui Li(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("li", attr, action);
		}

		#endregion

		#region main

		public static Ui BeginMain(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("main", attr);
		}

		public static Ui EndMain(this Ui ui)
		{
			return ui.EndElement("main");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginMain();
		///		action();
		///		ui.EndMain();
		///	</code>
		/// </summary>
		public static Ui Main(this Ui ui, Action action)
		{
			return ui.Element("main", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginMain(attr);
		///		action();
		///		ui.EndMain();
		///	</code>
		/// </summary>
		public static Ui Main(this Ui ui, object attr, Action action)
		{
			return ui.Element("main", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginMain();
		///		action(ui);
		///		ui.EndMain();
		///	</code>
		/// </summary>
		public static Ui Main(this Ui ui, Action<Ui> action)
		{
			return ui.Element("main", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginMain(attr);
		///		action(ui);
		///		ui.EndMain();
		///	</code>
		/// </summary>
		public static Ui Main(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("main", attr, action);
		}

		#endregion

		#region nav

		public static Ui BeginNav(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("nav", attr);
		}

		public static Ui EndNav(this Ui ui)
		{
			return ui.EndElement("nav");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginNav();
		///		action();
		///		ui.EndNav();
		///	</code>
		/// </summary>
		public static Ui Nav(this Ui ui, Action action)
		{
			return ui.Element("nav", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginNav(attr);
		///		action();
		///		ui.EndNav();
		///	</code>
		/// </summary>
		public static Ui Nav(this Ui ui, object attr, Action action)
		{
			return ui.Element("nav", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginNav();
		///		action(ui);
		///		ui.EndNav();
		///	</code>
		/// </summary>
		public static Ui Nav(this Ui ui, Action<Ui> action)
		{
			return ui.Element("nav", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginNav(attr);
		///		action(ui);
		///		ui.EndNav();
		///	</code>
		/// </summary>
		public static Ui Nav(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("nav", attr, action);
		}

		#endregion

		#region noscript

		public static Ui BeginNoscript(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("noscript", attr);
		}

		public static Ui EndNoscript(this Ui ui)
		{
			return ui.EndElement("noscript");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginNoscript();
		///		action();
		///		ui.EndNoscript();
		///	</code>
		/// </summary>
		public static Ui Noscript(this Ui ui, Action action)
		{
			return ui.Element("noscript", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginNoscript(attr);
		///		action();
		///		ui.EndNoscript();
		///	</code>
		/// </summary>
		public static Ui Noscript(this Ui ui, object attr, Action action)
		{
			return ui.Element("noscript", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginNoscript();
		///		action(ui);
		///		ui.EndNoscript();
		///	</code>
		/// </summary>
		public static Ui Noscript(this Ui ui, Action<Ui> action)
		{
			return ui.Element("noscript", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginNoscript(attr);
		///		action(ui);
		///		ui.EndNoscript();
		///	</code>
		/// </summary>
		public static Ui Noscript(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("noscript", attr, action);
		}

		#endregion

		#region ol

		public static Ui BeginOl(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("ol", attr);
		}

		public static Ui EndOl(this Ui ui)
		{
			return ui.EndElement("ol");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginOl();
		///		action();
		///		ui.EndOl();
		///	</code>
		/// </summary>
		public static Ui Ol(this Ui ui, Action action)
		{
			return ui.Element("ol", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginOl(attr);
		///		action();
		///		ui.EndOl();
		///	</code>
		/// </summary>
		public static Ui Ol(this Ui ui, object attr, Action action)
		{
			return ui.Element("ol", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginOl();
		///		action(ui);
		///		ui.EndOl();
		///	</code>
		/// </summary>
		public static Ui Ol(this Ui ui, Action<Ui> action)
		{
			return ui.Element("ol", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginOl(attr);
		///		action(ui);
		///		ui.EndOl();
		///	</code>
		/// </summary>
		public static Ui Ol(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("ol", attr, action);
		}

		#endregion

		#region p

		public static Ui BeginP(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("p", attr);
		}

		public static Ui EndP(this Ui ui)
		{
			return ui.EndElement("p");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginP();
		///		action();
		///		ui.EndP();
		///	</code>
		/// </summary>
		public static Ui P(this Ui ui, Action action)
		{
			return ui.Element("p", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginP(attr);
		///		action();
		///		ui.EndP();
		///	</code>
		/// </summary>
		public static Ui P(this Ui ui, object attr, Action action)
		{
			return ui.Element("p", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginP();
		///		action(ui);
		///		ui.EndP();
		///	</code>
		/// </summary>
		public static Ui P(this Ui ui, Action<Ui> action)
		{
			return ui.Element("p", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginP(attr);
		///		action(ui);
		///		ui.EndP();
		///	</code>
		/// </summary>
		public static Ui P(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("p", attr, action);
		}

		#endregion

		#region pre

		public static Ui BeginPre(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("pre", attr);
		}

		public static Ui EndPre(this Ui ui)
		{
			return ui.EndElement("pre");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginPre();
		///		action();
		///		ui.EndPre();
		///	</code>
		/// </summary>
		public static Ui Pre(this Ui ui, Action action)
		{
			return ui.Element("pre", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginPre(attr);
		///		action();
		///		ui.EndPre();
		///	</code>
		/// </summary>
		public static Ui Pre(this Ui ui, object attr, Action action)
		{
			return ui.Element("pre", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginPre();
		///		action(ui);
		///		ui.EndPre();
		///	</code>
		/// </summary>
		public static Ui Pre(this Ui ui, Action<Ui> action)
		{
			return ui.Element("pre", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginPre(attr);
		///		action(ui);
		///		ui.EndPre();
		///	</code>
		/// </summary>
		public static Ui Pre(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("pre", attr, action);
		}

		#endregion

		#region section

		public static Ui BeginSection(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("section", attr);
		}

		public static Ui EndSection(this Ui ui)
		{
			return ui.EndElement("section");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginSection();
		///		action();
		///		ui.EndSection();
		///	</code>
		/// </summary>
		public static Ui Section(this Ui ui, Action action)
		{
			return ui.Element("section", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginSection(attr);
		///		action();
		///		ui.EndSection();
		///	</code>
		/// </summary>
		public static Ui Section(this Ui ui, object attr, Action action)
		{
			return ui.Element("section", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginSection();
		///		action(ui);
		///		ui.EndSection();
		///	</code>
		/// </summary>
		public static Ui Section(this Ui ui, Action<Ui> action)
		{
			return ui.Element("section", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginSection(attr);
		///		action(ui);
		///		ui.EndSection();
		///	</code>
		/// </summary>
		public static Ui Section(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("section", attr, action);
		}

		#endregion

		#region table

		public static Ui BeginTable(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("table", attr);
		}

		public static Ui EndTable(this Ui ui)
		{
			return ui.EndElement("table");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginTable();
		///		action();
		///		ui.EndTable();
		///	</code>
		/// </summary>
		public static Ui Table(this Ui ui, Action action)
		{
			return ui.Element("table", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginTable(attr);
		///		action();
		///		ui.EndTable();
		///	</code>
		/// </summary>
		public static Ui Table(this Ui ui, object attr, Action action)
		{
			return ui.Element("table", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginTable();
		///		action(ui);
		///		ui.EndTable();
		///	</code>
		/// </summary>
		public static Ui Table(this Ui ui, Action<Ui> action)
		{
			return ui.Element("table", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginTable(attr);
		///		action(ui);
		///		ui.EndTable();
		///	</code>
		/// </summary>
		public static Ui Table(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("table", attr, action);
		}

		#endregion

		#region tfoot

		public static Ui BeginTfoot(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("tfoot", attr);
		}

		public static Ui EndTfoot(this Ui ui)
		{
			return ui.EndElement("tfoot");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginTfoot();
		///		action();
		///		ui.EndTfoot();
		///	</code>
		/// </summary>
		public static Ui Tfoot(this Ui ui, Action action)
		{
			return ui.Element("tfoot", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginTfoot(attr);
		///		action();
		///		ui.EndTfoot();
		///	</code>
		/// </summary>
		public static Ui Tfoot(this Ui ui, object attr, Action action)
		{
			return ui.Element("tfoot", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginTfoot();
		///		action(ui);
		///		ui.EndTfoot();
		///	</code>
		/// </summary>
		public static Ui Tfoot(this Ui ui, Action<Ui> action)
		{
			return ui.Element("tfoot", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginTfoot(attr);
		///		action(ui);
		///		ui.EndTfoot();
		///	</code>
		/// </summary>
		public static Ui Tfoot(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("tfoot", attr, action);
		}

		#endregion

		#region ul

		public static Ui BeginUl(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("ul", attr);
		}

		public static Ui EndUl(this Ui ui)
		{
			return ui.EndElement("ul");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginUl();
		///		action();
		///		ui.EndUl();
		///	</code>
		/// </summary>
		public static Ui Ul(this Ui ui, Action action)
		{
			return ui.Element("ul", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginUl(attr);
		///		action();
		///		ui.EndUl();
		///	</code>
		/// </summary>
		public static Ui Ul(this Ui ui, object attr, Action action)
		{
			return ui.Element("ul", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginUl();
		///		action(ui);
		///		ui.EndUl();
		///	</code>
		/// </summary>
		public static Ui Ul(this Ui ui, Action<Ui> action)
		{
			return ui.Element("ul", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginUl(attr);
		///		action(ui);
		///		ui.EndUl();
		///	</code>
		/// </summary>
		public static Ui Ul(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("ul", attr, action);
		}

		#endregion

		#region video

		public static Ui BeginVideo(this Ui ui, bool? contenteditable = null, bool? draggable = null, object attr = null)
		{
			return ui.BeginElement("video", attr);
		}

		public static Ui EndVideo(this Ui ui)
		{
			return ui.EndElement("video");
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginVideo();
		///		action();
		///		ui.EndVideo();
		///	</code>
		/// </summary>
		public static Ui Video(this Ui ui, Action action)
		{
			return ui.Element("video", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginVideo(attr);
		///		action();
		///		ui.EndVideo();
		///	</code>
		/// </summary>
		public static Ui Video(this Ui ui, object attr, Action action)
		{
			return ui.Element("video", attr, action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginVideo();
		///		action(ui);
		///		ui.EndVideo();
		///	</code>
		/// </summary>
		public static Ui Video(this Ui ui, Action<Ui> action)
		{
			return ui.Element("video", action);
		}

		/// <summary> This call is equivalent to: 
		///	<code>
		///		ui.BeginVideo(attr);
		///		action(ui);
		///		ui.EndVideo();
		///	</code>
		/// </summary>
		public static Ui Video(this Ui ui, object attr, Action<Ui> action)
		{
			return ui.Element("video", attr, action);
		}

		#endregion
	}
}

