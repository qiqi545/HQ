using System;
using Blowdart.UI.Web.Internal;

namespace Blowdart.UI.Web
{
    partial class HtmlExtensions
    {
        public static bool Button(this Ui ui, string text, Attributes attr = null)
        {
            return Clickable(ui, "button", text, attr);
        }

        internal static bool Clickable(Ui ui, string el, string text, Attributes attr = null)
        {
            ui.NextId();
            var id = ui.NextIdHash;
            Dom(ui).AppendTag(el, id, text);
            Scripts(ui).AppendEvent("click", id);
            return ui.Clicked.Contains(id);
        }
    }

    // This should be code-genned.
    partial class HtmlExtensions
    {
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
    }
}
