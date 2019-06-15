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
using Lime.Web.SemanticUi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Node.UI.Shared
{
    public class TopBar : UiComponent
    {
        public override async void Render(Ui ui)
        {
            var accessor = ui.Context.UiServices.GetRequiredService<IHttpContextAccessor>();
            var connection = accessor.HttpContext.Features.Get<IHttpConnectionFeature>();
            var local = $"{connection?.LocalIpAddress}:{connection?.LocalPort}";

            ui.BeginDiv(new {@class = "ui fixed inverted menu"});
            {
                ui.BeginDiv(new {@class = "ui container"});
                {
                    ui.BeginElement("a", new {href = "#", @class = "header item"});
                    {
                        ui.Element("img", "HQ",
                            new {@class = "logo", src = "assets/images/logo.png", style = "margin-right: 1.5em;"});
                    }
                    ui.EndElement("a");

                    ui.BeginA("item", id: "sidebar-btn");
                    ui.I(new {@class = "sidebar icon"});
                    ui.Literal("Menu");
                    ui.EndA();
                    ui.OnReady(@"
$('#sidebar-btn').click(function() {
    $('.ui.sidebar')
        .sidebar('toggle')
        .sidebar({ context: $('.ui.basic.section')});
});");
                    ui.BeginDiv(new {@class = "ui simple dropdown item"});
                    {
                        ui.Literal("Docs ");
                        ui.Element("i", new {@class = "dropdown icon"});
                        ui.BeginDiv(new {@class = "menu"});
                        {
                            ui.Div("Specs", new {@class = "header"});
                            ui.BeginA("item", href: "/meta/postman").Icon(BusinessIcons.FileOutline)
                                .Literal("Postman 2.1").EndA();
                            ui.BeginA("item", href: "/meta/swagger").Icon(BusinessIcons.FileOutline)
                                .Literal("Swagger 2.0").EndA();

                            ui.Div(new {@class = "divider"});

                            ui.Div("Help Pages", new {@class = "header"});
                            ui.BeginA("item", href: "/docs/swagger").Icon(BusinessIcons.Book).Literal("Swagger UI")
                                .EndA();
                        }
                        ui.EndDiv();
                    }
                    ui.EndDiv();

                    if (ui.Context.User.Identity.IsAuthenticated)
                    {
                        ui.BeginDiv(new {@class = "right menu"});
                        {
                            ui.BeginDiv("ui dropdown item");
                            {
                                ui.Literal("Language ");
                                ui.Element("i", new {@class = "dropdown icon"});
                                ui.BeginDiv(new {@class = "menu"});
                                {
                                    ui.BeginA("item", href: "#").Icon(BusinessIcons.Globe).Literal("English").EndA();
                                }
                                ui.EndDiv();
                            }
                            ui.EndDiv();

                            ui.BeginDiv("item");
                            {
                                var id = ui.NextId();
                                ui.Div("Sign Out", new {@class = "ui primary button", id = id.ToString()});
                                if (ui.OnClick())
                                {
                                    await accessor.HttpContext.SignOutAsync();
                                }
                            }
                            ui.EndDiv();
                        }
                        ui.EndDiv();
                    }
                }
                ui.EndDiv();
            }
            ui.EndDiv();
        }
    }
}
