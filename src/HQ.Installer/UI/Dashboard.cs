using Blowdart.UI;
using Blowdart.UI.Theming;
using Blowdart.UI.Web;
using Blowdart.UI.Web.SemanticUi;
using Blowdart.UI.Web.SemanticUI;
using Demo.UI.Shared;
using HQ.Installer.UI.Models;

namespace Demo.UI
{
    public class Dashboard : Admin<DashboardModel>
    {
        public override void Content(Ui ui, DashboardModel model)
        {
            ui.BeginDiv(new { @class = "ui main text container", style = "margin-top: 7em;" });

            var fte = model.TenantCount == 0;

            if (fte)
            {
               ui.BeginDiv("ui info message")
                    .Div("Welcome to HQ!", new {@class = "header"})
                    .BeginUl("list")
                    .Li("This HQ.io instance is a new install. You need to configure it before using it.")
                    .Li("If you intended to add this instance to an existing cluster, you may do some from <a href=\"/settings\">Settings</a>.")
                    .EndUl();
                ui.EndDiv();

                ui.BeginH4("ui horizontal divider header")
                    .Icon(InterfaceIcons.Cogs)
                    .Literal("New Cluster Setup")
                    .EndH4();

                ui.BeginDiv("ui three cards");

                //
                // Application:
                ui.BeginDiv("card");
                    ui.BeginDiv("content");
                        ui.Div("Create an Application", new {@class = "header"});
                        ui.Div("An application is a logical group of user roles and object schemas.", new { @class = "description"});
                    ui.EndDiv();
                    ui.BeginDiv("ui bottom attached button").Icon(FontAwesomeIcons.PlusCircle, NamedColors.Green).Literal("Add Application");
                    ui.EndDiv();
                ui.EndDiv();

                //
                // Access Token:
                ui.BeginDiv("card");
                    ui.BeginDiv("content");
                        ui.Div("Create an Access Token", new { @class = "header" });
                        ui.Div("You need an access token or super user account to create users and assign roles.", new { @class = "description" });
                    ui.EndDiv();
                ui.BeginDiv("ui bottom attached button").Icon(FontAwesomeIcons.PlusCircle, NamedColors.Green).Literal("Create Token");
                ui.EndDiv();
                ui.EndDiv();

                //
                // Configure Settings:
                ui.BeginDiv("card");
                    ui.BeginDiv("content");
                        ui.BeginDiv("header").Literal("Configure Cluster Settings").EndDiv();
                        ui.Div("Create a configuration to manage your cluster's features and security.", new { @class = "description" });
                    ui.EndDiv();
                    ui.BeginDiv("ui bottom attached button").Icon(FontAwesomeIcons.ArrowCircleRight, NamedColors.Green).Literal("Settings");
                ui.EndDiv();
                ui.EndDiv();
                
                ui.EndDiv(); // cards


                ui.BeginH4("ui horizontal divider header")
                    .Icon(FontAwesomeIcons.ChartBar)
                    .Literal("Diagnostics")
                    .EndH4();

                ui.Component<LogStreamViewer>();
            }

            ui.EndDiv();
        }
    }
}
