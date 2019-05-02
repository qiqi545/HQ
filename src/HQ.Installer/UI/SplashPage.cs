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

using System.Reflection;
using Blowdart.UI;
using Blowdart.UI.Web;
using HQ.Platform.Schema.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Installer.UI
{
    public class SplashPage : UiComponent
    {
        public override void Render(Ui ui)
        {
            var accessor = ui.GetRequiredService<IHttpContextAccessor>();
            var connection = accessor.HttpContext.Features.Get<IHttpConnectionFeature>();
            var local = $"{connection?.LocalIpAddress}:{connection?.LocalPort}";

            //
            // Top Bar Menu:
            //
            ui.BeginDiv(new { @class = "ui fixed inverted menu" });
            {
                ui.BeginDiv(new { @class = "ui container"});
                {
                    ui.BeginElement("a", new { href = "#", @class = "header item"});
                    {
                        ui.Element("img", "HQ", new { @class = "logo", src = "assets/images/logo.png", style= "margin-right: 1.5em;" });
                    }
                    ui.EndElement("a");
                    ui.Element("a", $"{local}", new { href = "#", @class = "item"});

                    /*
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
                    */
                }
                ui.EndDiv();
            }
            ui.EndDiv();

            //
            // Main:
            //
            ui.BeginDiv(new { @class = "ui main text container", style = "margin-top: 7em;" });
            {
                ui.BeginDiv(new { @class = "ui four column grid"});
                {
                    ui.BeginDiv(new { @class = "column"});
                    {
                        ui.Element("h1", "Server Logs");
                    }
                    ui.EndDiv();
                    ui.BeginDiv(new { @class = "column" });
                    {
                        ui.BeginElement("button", new { @class = "ui labeled small icon button", id="clear-logs" });
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
                ui.BeginDiv(new { @class = "ui segment"});
                {
                    ui.BeginDiv(new { @class = "ui active dimmer", id="no-logs"});
                    {
                         ui.Div("Tailing logs...", new { @class = "ui indeterminate text loader" });
                    }
                    ui.EndDiv();
                    ui.BeginDiv(new { id = "logs", @class = "ui relaxed divided list", style = "height: 300px; overflow: auto;" });
                    ui.EndDiv();
                }
                ui.EndDiv();
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
                            ui.Element("h4", "Platform", new { @class = "ui inverted header"});
                            ui.BeginDiv(new { @class = "ui inverted link list"});
                            {
                                ui.Element("a", "Security", new { href="#", @class = "item"});
                                ui.Element("a", "Pricing", new { href = "#", @class = "item" });
                                ui.Element("a", "Integrations", new { href = "#", @class = "item" });
                                ui.Element("a", "Documentations", new { href = "#", @class = "item" });
                            }
                            ui.EndDiv();
                        }
                        ui.EndDiv();

                        ui.BeginDiv(new { @class = "three wide column" });
                        {
                            ui.Element("h4", "Corporate", new { @class = "ui inverted header" });
                            ui.BeginDiv(new { @class = "ui inverted link list" });
                            {
                                ui.Element("a", "About", new { href = "#", @class = "item" });
                                ui.Element("a", "Jobs", new { href = "#", @class = "item" });
                                ui.Element("a", "Blog", new { href = "#", @class = "item" });
                                ui.Element("a", "Press", new { href = "#", @class = "item" });
                                ui.Element("a", "Partners", new { href = "#", @class = "item" });
                            }
                            ui.EndDiv();
                        }
                        ui.EndDiv();

                        ui.BeginDiv(new { @class = "three wide column" });
                        {
                            ui.Element("h4", "Community", new { @class = "ui inverted header" });
                            ui.BeginDiv(new { @class = "ui inverted link list" });
                            {
                                ui.Element("a", "Events", new { href = "#", @class = "item" });
                                ui.Element("a", "Case Studies", new { href = "#", @class = "item" });
                            }
                            ui.EndDiv();
                        }
                        ui.EndDiv();

                        ui.BeginDiv(new { @class = "seven wide column" });
                        {
                            ui.Element("h4", "Build Info", new { @class = "ui inverted header" });
                            ui.BeginDiv(new { @class = "text container" });
                            {
                                ui.Element("p", $"UI Version: {typeof(SplashPage).Assembly.GetName().Version}");
                                ui.Element("p", $"Platform Version: {typeof(Schema).Assembly.GetName().Version}");
                                ui.Element("p", $"App Version: {Assembly.GetEntryAssembly().GetName().Version}");
                            }
                            ui.EndDiv();
                        }
                        ui.EndDiv();
                    }
                    ui.EndDiv();

                    ui.Element("div", attr: new { @class = "ui inverted section divider"});
                    ui.Element("img", attr: new { src = "assets/images/logo.png", @class = "ui centered mini image"});
                    ui.BeginDiv(new { @class = "ui horizontal inverted small divided link list"});
                    {
                        ui.Element("a", "Privacy Policy", new { href = "https://hq.io/privacy", @class = "item" });
                        ui.Element("a", "Terms & Conditions", new { href = "https://hq.io/toc", @class = "item" });
                        ui.BeginElement("a", new { href = "https://github.com/hq-io", @class = "item" });
                        {
                            ui.Literal("Open Source ");
                            ui.Element("i", "", new { @class = "github icon"});
                        }
                        ui.EndElement("a");
                    }
                    ui.EndDiv();
                }
                ui.EndDiv();
            }
            ui.EndDiv();
        }
    }
}
