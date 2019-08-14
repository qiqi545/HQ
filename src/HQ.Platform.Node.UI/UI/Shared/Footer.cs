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
using HQ.Common;
using HQ.Platform.Node.UI.Pages;
using HQ.UI;
using HQ.UI.Web;

namespace HQ.Platform.Node.UI.Shared
{
    public class Footer : UiComponent
    {
        public override void Render(Ui ui)
        {
            ui.BeginDiv(new {@class = "ui inverted footer segment", style = "margin: 5em 0em 0em; padding: 5em 0em;"});
            {
                ui.BeginDiv(new {@class = "ui center aligned container"});
                {
                    ui.BeginDiv(new {@class = "ui stackable inverted divided grid"});
                    {
                        ui.BeginDiv(new {@class = "three wide column"});
                        {
                            ui.Element("h4", "Platform", new {@class = "ui inverted header"});
                            ui.BeginDiv(new {@class = "ui inverted link list"});
                            {
                                ui.Element("a", "Security", new {href = "#", @class = "item"});
                                ui.Element("a", "Pricing", new {href = "#", @class = "item"});
                                ui.Element("a", "Integrations", new {href = "#", @class = "item"});
                                ui.Element("a", "Documentations", new {href = "#", @class = "item"});
                            }
                            ui.EndDiv();
                        }
                        ui.EndDiv();

                        ui.BeginDiv(new {@class = "three wide column"});
                        {
                            ui.Element("h4", "Corporate", new {@class = "ui inverted header"});
                            ui.BeginDiv(new {@class = "ui inverted link list"});
                            {
                                ui.Element("a", "About", new {href = "#", @class = "item"});
                                ui.Element("a", "Jobs", new {href = "#", @class = "item"});
                                ui.Element("a", "Blog", new {href = "#", @class = "item"});
                                ui.Element("a", "Press", new {href = "#", @class = "item"});
                                ui.Element("a", "Partners", new {href = "#", @class = "item"});
                            }
                            ui.EndDiv();
                        }
                        ui.EndDiv();

                        ui.BeginDiv(new {@class = "three wide column"});
                        {
                            ui.Element("h4", "Community", new {@class = "ui inverted header"});
                            ui.BeginDiv(new {@class = "ui inverted link list"});
                            {
                                ui.Element("a", "Events", new {href = "#", @class = "item"});
                                ui.Element("a", "Case Studies", new {href = "#", @class = "item"});
                            }
                            ui.EndDiv();
                        }
                        ui.EndDiv();

                        ui.BeginDiv(new {@class = "seven wide column"});
                        {
                            ui.Element("h4", "Build Info", new {@class = "ui inverted header"});
                            ui.BeginDiv(new {@class = "text container"});
                            {
                                ui.Element("p", $"UI Version: {typeof(Dashboard).Assembly.GetName().Version}");
                                ui.Element("p",
                                    $"Platform Version: {typeof(Constants.Schemas).Assembly.GetName().Version}");
                                ui.Element("p", $"App Version: {Assembly.GetEntryAssembly().GetName().Version}");
                            }
                            ui.EndDiv();
                        }
                        ui.EndDiv();
                    }
                    ui.EndDiv();

                    ui.Element("div", new {@class = "ui inverted section divider"});
                    ui.Element("img", new {src = "assets/images/logo.png", @class = "ui centered mini image"});
                    ui.BeginDiv(new {@class = "ui horizontal inverted small divided link list"});
                    {
                        ui.Element("a", "Privacy Policy", new {href = "https://hq.io/privacy", @class = "item"});
                        ui.Element("a", "Terms & Conditions", new {href = "https://hq.io/toc", @class = "item"});
                        ui.BeginElement("a", new {href = "https://github.com/hq-io", @class = "item"});
                        {
                            ui.Literal("Open Source ");
                            ui.Element("i", "", new {@class = "github icon"});
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
