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

using HQ.UI;
using HQ.UI.Web;
using HQ.UI.Web.SemanticUi;

namespace HQ.Platform.Node.UI.Shared
{
    public class Sidebar : UiComponent
    {
        public override void Render(Ui ui)
        {
            ui.BeginDiv(new {@class = "ui sidebar inverted vertical menu"});
            ui.BeginDiv("item")

                //
                // Dashboard:
                .BeginDiv("header")
                .Icon(FontAwesomeIcons.Windows).A("Dashboard", new {href = "/", @class = "menu item active"})
                .Literal("Dashboard")
                .EndDiv()

                //
                // Users:
                .BeginDiv("header").Icon(FontAwesomeIcons.Users).Literal("Users").EndDiv()
                .BeginDiv("menu")
                .BeginA("item").Icon(FontAwesomeIcons.Gamepad).Literal("Games").EndA()
                .BeginA("item").Icon(FontAwesomeIcons.Gamepad).Literal("Games").EndA()
                .BeginA("item").Icon(FontAwesomeIcons.Gamepad).Literal("Games").EndA()
                .EndDiv()

                //
                // Operations:
                .BeginDiv("header").Icon(FontAwesomeIcons.Diagnoses).Literal("Operations").EndDiv()
                .BeginDiv("menu")
                .BeginA("item").Icon(FontAwesomeIcons.Gamepad).Literal("Games").EndA()
                .BeginA("item").Icon(FontAwesomeIcons.Gamepad).Literal("Games").EndA()
                .BeginA("item").Icon(FontAwesomeIcons.Gamepad).Literal("Games").EndA()
                .EndDiv()

                //
                // Settings:
                .BeginDiv("header").Icon(FontAwesomeIcons.ObjectGroup).Literal("Settings").EndDiv()
                .BeginDiv("menu")
                .BeginA("item").Icon(FontAwesomeIcons.Gamepad).Literal("Games").EndA()
                .BeginA("item").Icon(FontAwesomeIcons.Gamepad).Literal("Games").EndA()
                .BeginA("item").Icon(FontAwesomeIcons.Gamepad).Literal("Games").EndA()
                .EndDiv()

                //
                // Log Stream:
                .BeginDiv("header menu item")
                .Icon(FontAwesomeIcons.Bars).A("Log Stream", new {href = "/logstream"})
                .EndDiv();

            ui.EndDiv();
            ui.EndDiv();
        }
    }
}
