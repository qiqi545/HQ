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
    public class LogStreamViewer : UiComponent
    {
        public override void Render(Ui ui)
        {
            ui.BeginDiv(new { @class = "ui four column grid" });
            {
                ui.BeginDiv(new { @class = "column" });
                {
                    ui.Element("h1", "Log Stream");
                }
                ui.EndDiv();
                ui.BeginDiv(new { @class = "column" });
                {
                    ui.BeginElement("button", new { @class = "ui labeled small icon button", id = "clear-logs" });
                    {
                        ui.Element("i", "", new { @class = "trash icon" });
                        ui.Literal("Clear");
                    }
                    ui.EndElement("button");
                }
                ui.EndDiv();
            }
            ui.EndDiv();

            ui.Element("p", "");
            ui.BeginDiv(new { @class = "ui segment" });
            {
                ui.BeginDiv(new { @class = "ui active dimmer", id = "no-logs" });
                {
                    ui.Div("Waiting for logs...", new { @class = "ui indeterminate text loader" });
                }
                ui.EndDiv();
                ui.BeginDiv(new
                    { id = "logs", @class = "ui relaxed divided list", style = "height: 300px; overflow: auto;" });
                ui.EndDiv();
            }
            ui.EndDiv();
        }
    }
}
