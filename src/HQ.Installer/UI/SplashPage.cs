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

using Blowdart.UI;
using Blowdart.UI.Web;

namespace HQ.Installer.UI
{
    public class SplashPage : UiComponent
    {
        public override void Render(Ui ui)
        {
            //
            // Top Bar Menu:
            //
            ui.BeginDiv(new { @class = "ui fixed inverted menu" });
            {
                ui.BeginDiv(new { @class = "ui container"});
                {
                    ui.BeginElement("a", new { href = "#", @class = "header item"});
                    {
                        ui.Element("img", "HQ.Template", new { @class = "logo", src = "assets/images/logo.png", style= "margin-right: 1.5em;" });
                    }
                    ui.EndElement("a");
                    ui.Element("a", "Home", new { href = "#", @class = "item"});
                    ui.BeginDiv(new { @class = "ui simple dropdown item" });
                    {
                        ui.Literal("Dropdown ");
                        ui.Element("i", attr: new { @class = "dropdown icon" });
                        ui.BeginDiv(new {@class = "menu"});
                        {
                            ui.Element("a", "Link Item", new { href = "#", @class = "item" });
                            ui.Element("a", "Link Item", new { href = "#", @class = "item" });
                            ui.Div(attr: new { @class = "divider" });

                            ui.Div("Header Item", new { @class = "header" });
                            ui.BeginDiv(attr: new { @class = "item"});
                            {
                                ui.Element("i", attr: new { @class = "dropdown icon" });
                                ui.Literal("Sub Menu");
                                ui.BeginDiv(new {@class = "menu"});
                                {
                                    ui.Element("a", "Link Item", new { href = "#", @class = "item" });
                                    ui.Element("a", "Link Item", new { href = "#", @class = "item" });
                                }
                                ui.EndDiv();
                            }
                            ui.EndDiv();

                            ui.Element("a", "Link Item", new { href = "#", @class = "item" });
                        }
                        ui.EndDiv();
                    }
                    ui.EndDiv();
                }
                ui.EndDiv();
            }
            ui.EndDiv();

            //
            // Main:
            //
            ui.BeginDiv(new { @class = "ui main text container", style = "margin-top: 7em;" });
            {
                ui.Element("h1", "Semantic UI Fixed Template");
                ui.Element("p", "This is a basic fixed menu template using fixed size containers.");
                ui.Element("p", "A text container is used for the main container, which is useful for single column layouts.");
            }
            ui.EndDiv();

            //
            // Footer:
            //
            ui.BeginDiv(new { @class = "ui inverted footer segment", style = "margin: 5em 0em 0em; padding: 5em 0em;"});
            {
                ui.BeginDiv(new { @class = "ui center aligned container" });
                {
                    ui.BeginDiv(new { @class = "ui stackable inverted divided grid" });
                    {
                        ui.BeginDiv(new { @class = "three wide column" });
                        {
                            ui.Element("h4", "Group 1", new { @class = "ui inverted header"});
                            ui.BeginDiv(new { @class = "ui inverted link list"});
                            {
                                ui.Element("a", "Link One", new { href="#", @class = "item"});
                                ui.Element("a", "Link Two", new { href = "#", @class = "item" });
                                ui.Element("a", "Link Three", new { href = "#", @class = "item" });
                                ui.Element("a", "Link Four", new { href = "#", @class = "item" });
                            }
                            ui.EndDiv();
                        }
                        ui.EndDiv();

                        ui.BeginDiv(new { @class = "three wide column" });
                        {
                            ui.Element("h4", "Group 2", new { @class = "ui inverted header" });
                            ui.BeginDiv(new { @class = "ui inverted link list" });
                            {
                                ui.Element("a", "Link One", new { href = "#", @class = "item" });
                                ui.Element("a", "Link Two", new { href = "#", @class = "item" });
                                ui.Element("a", "Link Three", new { href = "#", @class = "item" });
                                ui.Element("a", "Link Four", new { href = "#", @class = "item" });
                            }
                            ui.EndDiv();
                        }
                        ui.EndDiv();

                        ui.BeginDiv(new { @class = "three wide column" });
                        {
                            ui.Element("h4", "Group 3", new { @class = "ui inverted header" });
                            ui.BeginDiv(new { @class = "ui inverted link list" });
                            {
                                ui.Element("a", "Link One", new { href = "#", @class = "item" });
                                ui.Element("a", "Link Two", new { href = "#", @class = "item" });
                                ui.Element("a", "Link Three", new { href = "#", @class = "item" });
                                ui.Element("a", "Link Four", new { href = "#", @class = "item" });
                            }
                            ui.EndDiv();
                        }
                        ui.EndDiv();

                        ui.BeginDiv(new { @class = "seven wide column" });
                        {
                            ui.Element("h4", "Footer Header", new { @class = "ui inverted header" });
                            ui.Element("p", "Extra space for a call to action inside the footer that could help re-engage users.");
                        }
                        ui.EndDiv();
                    }
                    ui.EndDiv();

                    ui.Div(attr: new { @class = "ui inverted section divider"});
                    ui.Element("img", attr: new { src = "assets/images/logo.png", @class = "ui centered mini image"});
                    ui.BeginDiv(new { @class = "ui horizontal inverted small divided link list"});
                    {
                        ui.Element("a", "About Us", new { href = "#", @class = "item" });
                        ui.Element("a", "Contact Us", new { href = "#", @class = "item" });
                        ui.Element("a", "Terms & Conditions", new { href = "#", @class = "item" });
                        ui.Element("a", "Open Source", new { href = "#", @class = "item" });
                    }
                    ui.EndDiv();
                }
                ui.EndDiv();
            }
            ui.EndDiv();
        }
    }
}
