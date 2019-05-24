#region LICENSE
// Unless explicitly acquired and licensed from HQ.IO Corporation 
// under another license, the contents of this file are copyrighted, 
// and you are strictly prohibited from using it for any purpose. 
// Violations will be prosecuted under the full extent the law allows.
#endregion

using Blowdart.UI;
using Blowdart.UI.Web;
using Blowdart.UI.Web.SemanticUI;

namespace Demo.UI.Shared
{
    public class Sidebar : UiComponent
    {
        public override void Render(Ui ui)
        {
            ui.BeginDiv(new { @class = "ui sidebar inverted vertical menu" });
                ui.BeginDiv("item")

                    //
                    // Dashboard:
                    .BeginDiv("header")
                    .Icon(FontAwesomeIcons.Windows).A("Dashboard", new { href = "/", @class = "menu item active" })
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
                    .Icon(FontAwesomeIcons.Bars).A("Log Stream", new { href = "/logstream"})
                    .EndDiv();
                    
                ui.EndDiv();
            ui.EndDiv();
        }
    }
}
