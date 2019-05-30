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
