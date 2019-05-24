#region LICENSE
// Unless explicitly acquired and licensed from HQ.IO Corporation 
// under another license, the contents of this file are copyrighted, 
// and you are strictly prohibited from using it for any purpose. 
// Violations will be prosecuted under the full extent the law allows.
#endregion

using Blowdart.UI;
using Blowdart.UI.Web;
using Blowdart.UI.Web.SemanticUi;
using Blowdart.UI.Web.SemanticUI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.UI.Shared
{
    public class TopBar : UiComponent
    {
        public override void Render(Ui ui)
        {
            var accessor = ui.GetRequiredService<IHttpContextAccessor>();
            var connection = accessor.HttpContext.Features.Get<IHttpConnectionFeature>();
            var local = $"{connection?.LocalIpAddress}:{connection?.LocalPort}";


            ui.BeginDiv(new { @class = "ui fixed inverted menu" });
            {
                ui.BeginDiv(new { @class = "ui container" });
                {
                    ui.BeginElement("a", new { href = "#", @class = "header item" });
                    {
                        ui.Element("img", "HQ",
                            new { @class = "logo", src = "assets/images/logo.png", style = "margin-right: 1.5em;" });
                    }
                    ui.EndElement("a");

                    /* TODO activate when not FTE
                    ui.BeginA("item", id: "sidebar-btn");
                    ui.I(new { @class = "sidebar icon" });
                    ui.Literal("Menu");
                    ui.EndA();
                    ui.OnReady(@"
$('#sidebar-btn').click(function() {
    $('.ui.sidebar')
        .sidebar('toggle')
        //.sidebar({ context: $('.ui.basic.section')});
});");
                    */

                    // ui.Element("a", $"{local}", new { href = "#", @class = "item" });

                    ui.BeginDiv(new { @class = "ui simple dropdown item" });
                    {
                        ui.Literal("Docs ");
                        ui.Element("i", new { @class = "dropdown icon" });
                        ui.BeginDiv(new { @class = "menu" });
                        {
                            ui.Div("Specs", new { @class = "header" });
                            ui.BeginA("item", href: "/meta/postman").Icon(BusinessIcons.FileOutline).Literal("Postman 2.1").EndA();
                            ui.BeginA("item", href: "/meta/swagger").Icon(BusinessIcons.FileOutline).Literal("Swagger 2.0").EndA();

                            ui.Div(new { @class = "divider" });

                            ui.Div("Help Pages", new { @class = "header" });
                            ui.BeginA("item", href: "/docs/swagger").Icon(BusinessIcons.Book).Literal("Swagger UI").EndA();
                        }
                        ui.EndDiv();
                    }
                    ui.EndDiv();
                }
                ui.EndDiv();
            }
            ui.EndDiv();
        }
    }
}
