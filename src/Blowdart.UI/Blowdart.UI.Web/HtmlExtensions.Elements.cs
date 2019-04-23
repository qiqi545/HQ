using System;
using Blowdart.UI.Web.Internal;

namespace Blowdart.UI.Web
{
    partial class HtmlExtensions
    {
        public static bool Button(this Ui ui, string text, object attr = null)
        {
            return Clickable(ui, "button", text, attr);
        }

        internal static bool Clickable(Ui ui, string el, string text, object attr = null)
        {
            ui.NextId();
            var id = ui.NextIdHash;
            Dom(ui).AppendTag(el, id, text, attr == null ? null : Attr(attr));
            Scripts(ui).AppendEvent("click", id);
            return ui.Clicked.Contains(id);
        }
    }

    // This should be code-genned.
    partial class HtmlExtensions
    {
        #region span

        public static Ui BeginSpan(this Ui ui, object attr = null)
        {
            ui.BeginElement("span", attr);
            return ui;
        }

        public static Ui EndSpan(this Ui ui)
        {
            ui.EndElement("span");
            return ui;
        }

        public static Ui Span(this Ui ui, object attr = null, Action action = null)
        {
            ui.Element("span", attr, action);
            return ui;
        }

        public static Ui Span(this Ui ui, object attr = null, Action<Ui> action = null)
        {
            ui.Element("span", attr, action);
            return ui;
        }

        public static Ui Span(this Ui ui, string innerText, object attr = null)
        {
            ui.Element("span", innerText, attr);
            return ui;
        }

        #endregion

        #region div
		
        public static Ui Div(this Ui ui, string innerText, object attr = null)
        {
            ui.Element("div", innerText, attr);
            return ui;
        }

        #endregion

        #region p 

        public static Ui P(this Ui ui, string innerText, object attr = null)
        {
            ui.Element("p", innerText, attr);
            return ui;
        }

        #endregion

        #region a 

        public static Ui BeginA(this Ui ui, object attr = null)
        {
            ui.BeginElement("a", attr);
            return ui;
        }

        public static Ui EndA(this Ui ui)
        {
            ui.EndElement("a");
            return ui;
        }

        public static Ui A(this Ui ui, object attr = null, Action action = null)
        {
            ui.Element("a", attr, action);
            return ui;
        }

        public static Ui A(this Ui ui, object attr = null, Action<Ui> action = null)
        {
            ui.Element("a", attr, action);
            return ui;
        }

        public static Ui A(this Ui ui, string innerText, object attr = null)
        {
            ui.Element("p", innerText, attr);
            return ui;
        }

        #endregion

        #region img 

        public static Ui BeginImg(this Ui ui, object attr = null)
        {
            ui.BeginElement("img", attr);
            return ui;
        }

        public static Ui EndImg(this Ui ui)
        {
            ui.EndElement("img");
            return ui;
        }

        public static Ui Img(this Ui ui, object attr = null, Action action = null)
        {
            ui.Element("img", attr, action);
            return ui;
        }

        public static Ui Img(this Ui ui, object attr = null, Action<Ui> action = null)
        {
            ui.Element("img", attr, action);
            return ui;
        }

        public static Ui Img(this Ui ui, string innerText, object attr = null)
        {
            ui.Element("img", innerText, attr);
            return ui;
        }

        #endregion

        #region pre

        public static Ui Pre(this Ui ui, string innerText, object attr = null)
        {
            ui.Element("pre", innerText, attr);
            return ui;
        }

        #endregion
    }
}
