using System;
using HQ.Data.Sql.Sqlite;
using HQ.Extensions.Options;
using HQ.Platform.Identity.Models;
using HQ.Platform.Security.Configuration;
using Lime;
using Lime.Theming;
using Lime.Web;
using Lime.Web.SemanticUi;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HQ.Template.UI.Models;
using HQ.Template.UI.Shared;

namespace HQ.Template.UI.Pages
{
    public class Dashboard : Admin<DashboardModel>
    {
        public override void Content(Ui ui, DashboardModel model)
        {
            ui.BeginDiv(new { @class = "ui main text container", style = "margin-top: 7em;" });

            if (model.IsFirstTimeExperience)
            {
                FirstTimeExperience(ui, model);
            }
            else
            {

            }

            ui.EndDiv();
        }

        private static void FirstTimeExperience(Ui ui, DashboardModel model)
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
            {
                CreateAppBox(ui, model);
                CreateSettingsBox(ui, model);
                CreateAccessTokenBox(ui, model);
            }
            ui.EndDiv(); // cards

            ui.BeginH4("ui horizontal divider header")
                .Icon(FontAwesomeIcons.ChartBar)
                .Literal("Diagnostics")
                .EndH4();

            ui.Component<LogStreamViewer>();
        }

        private static void CreateAppBox(Ui ui, DashboardModel model)
        {
            var id = ui.NextId("apps").ToString();

            var message = model.HasApplication
                ? "You have at least one application ready to serve API objects."
                : "An application is a logical group of user roles and object schemas.";
            var icon = model.HasApplication ? FontAwesomeIcons.CheckCircle : FontAwesomeIcons.PlusCircle;
            var color = model.HasApplication ? NamedColors.Grey : NamedColors.Green;
            var buttonState = model.HasApplication ? " disabled" : "";

            ui.BeginDiv("card");
            {
                ui.BeginDiv("content");
                {
                    ui.Div("Create an Application", new { @class = "header" });
                    ui.Div(message, new { @class = $"description" });
                }
                ui.EndDiv();

                ui.BeginDiv($"ui bottom attached button{buttonState}", id: id)
                    .Icon(icon, color).Literal("Add Application")
                    .EndDiv();
            }
            ui.EndDiv();

            if (ui.OnClick())
            {
                var appService = ui.Context.UiServices.GetRequiredService<IApplicationService<IdentityApplication>>();
                var any = appService.GetAsync().GetAwaiter().GetResult();
                if (any.Succeeded && any.Data.Any())
                    return;
                appService.CreateAsync(new CreateApplicationModel { Name = "Default Application" }).GetAwaiter().GetResult();
                ui.Invalidate();
            }
        }

        private static void CreateAccessTokenBox(Ui ui, DashboardModel model)
        {
            var id = ui.NextId("tokens").ToString();

            var message = model.HasAccessToken
                ? "You're ready to access this secured node."
                : "You need an access token or super user account to create users and assign roles.";
            var icon = model.HasAccessToken ? FontAwesomeIcons.CheckCircle : FontAwesomeIcons.PlusCircle;
            var color = model.HasAccessToken ? NamedColors.Grey : NamedColors.Green;
            var buttonState = model.HasAccessToken ? " disabled" : "";
            
            ui.BeginDiv($"card");
            {
                ui.BeginDiv($"content");
                {
                    ui.Div("Create an Access Token", new { @class = $"header" });
                    ui.Div(message, new { @class = $"description" });
                }
                ui.EndDiv();

                ui.BeginDiv($"ui bottom attached button{buttonState}", id: id)
                    .Icon(icon, color).Literal("Create Token")
                    .EndDiv();
            }
            ui.EndDiv();

            if(ui.OnClick())
            {
                try
                {
                    var options = ui.Context.UiServices.GetService<ISaveOptions<SecurityOptions>>();
                    options.TrySave("HQ:Security", x =>
                    {
                        x.SuperUser.Enabled = true;
                        x.SuperUser.Username = "darthvader";
                        x.SuperUser.Password = "deathstar";
                        x.SuperUser.Email = "darthvader@deathstar.com";
                        x.SuperUser.PhoneNumber = "5555555555";
                    });
                    var configRoot = ui.Context.UiServices.GetRequiredService<IConfigurationRoot>();
                    configRoot.Reload();
                    ui.Invalidate();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static void CreateSettingsBox(Ui ui, DashboardModel model)
        {
            var completed = model.HasSettings;
            var id = ui.NextId("settings").ToString();
            var message = completed
                ? "You have successfully initialized your configuration settings."
                : "Create a configuration to manage your cluster's features and security.";
            var icon = completed ? FontAwesomeIcons.CheckCircle : FontAwesomeIcons.PlusCircle;
            var color = completed ? NamedColors.Grey : NamedColors.Green;
            var buttonState = completed ? " disabled" : "";
            
            ui.BeginDiv($"card");
            {
                ui.BeginDiv($"content");
                {
                    ui.Div("Configure Cluster Settings", new { @class = $"header" });
                    ui.Div(message, new { @class = $"description" });
                }
                ui.EndDiv();

                ui.BeginDiv($"ui bottom attached button{buttonState}", id: id)
                    .Icon(icon, color).Literal("Create Settings")
                    .EndDiv();
            }
            ui.EndDiv();

            if (ui.OnClick())
            {
                if (!SqliteConfigurationHelper.IsEmptyConfiguration("settings.db"))
                    return;
                var configSeed = ConfigurationLoader.FromEmbeddedJsonFile("seed.json");
                if (configSeed == null)
                    return;
                SqliteConfigurationHelper.MigrateToLatest("settings.db", configSeed);
                var configRoot = ui.Context.UiServices.GetRequiredService<IConfigurationRoot>();
                configRoot.Reload();
                ui.Invalidate();
            }
        }
    }
}
