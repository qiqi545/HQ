#region LICENSE

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
using Lime;
using Lime.Web;

namespace HQ.Template.UI.Shared
{
    public abstract class Admin<TModel> : HtmlComponent<TModel>
    {
        public abstract void Content(Ui ui, TModel model);

        public override void Render(Ui ui, TModel model)
        {
            ui.Component<Sidebar>();
            ui.Component<TopBar>();

            ui.BeginDiv(new {@class = "pusher"});
            ui.BeginDiv("ui basic section");

            Content(ui, model);

            ui.EndDiv();
            ui.EndDiv();

            ui.Component<Footer>();
        }
    }

    public abstract class Admin : HtmlComponent
    {
        public abstract void Content(Ui ui);

        public override void Render(Ui ui)
        {
            ui.Component<Sidebar>();
            ui.Component<TopBar>();

            ui.BeginDiv(new { @class = "pusher" });
            ui.BeginDiv("ui basic section");

            Content(ui);

            ui.EndDiv();
            ui.EndDiv();

            ui.Component<Footer>();
        }
    }
}
