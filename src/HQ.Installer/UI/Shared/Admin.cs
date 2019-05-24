#region LICENSE
// Unless explicitly acquired and licensed from HQ.IO Corporation 
// under another license, the contents of this file are copyrighted, 
// and you are strictly prohibited from using it for any purpose. 
// Violations will be prosecuted under the full extent the law allows.
#endregion

using Blowdart.UI;
using Blowdart.UI.Web;

namespace Demo.UI.Shared
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

        public abstract void Content(Ui ui);
    }
}
