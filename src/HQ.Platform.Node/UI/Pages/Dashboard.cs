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

using System;
using System.Linq;
using HQ.Extensions.Options;
using HQ.Integration.Sqlite.Sql;
using HQ.Platform.Identity.Models;
using HQ.Platform.Node.UI.Models;
using HQ.Platform.Node.UI.Shared;
using HQ.Platform.Security.AspNetCore.Models;
using HQ.Platform.Security.Configuration;
using HQ.UI;
using HQ.UI.Theming;
using HQ.UI.Web;
using HQ.UI.Web.SemanticUi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Node.UI.Pages
{
    public class Dashboard : Admin<DashboardModel>
    {
        public override void Content(Ui ui, DashboardModel model)
        {
            ui.BeginDiv(new {@class = "ui main text container", style = "margin-top: 7em;"});

            if (model.IsFirstTimeExperience)
            {
                FirstTimeExperience(ui, model);
            }

            ui.EndDiv();
        }

        private static void FirstTimeExperience(Ui ui, DashboardModel model)
        {
            ui.BeginDiv("ui info message")
                .Div("Welcome to HQ!", new {@class = "header"})
                .BeginUl("list")
                .Li("This HQ.io instance is a new install. You need to configure it before using it.")
                .Li(
                    "If you intended to add this instance to an existing cluster, you may do some from <a href=\"/settings\">Settings</a>.")
                .EndUl();
            ui.EndDiv();

            ui.BeginH4("ui horizontal divider header")
                .Icon(InterfaceIcons.Cogs)
                .Literal("New Cluster Setup")
                .EndH4();

            ui.BeginDiv("ui three cards");
            {
                CreateSettingsBox(ui, model);
                CreateAppBox(ui, model);
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
                    ui.Div("Create an Application", new {@class = "header"});
                    ui.Div(message, new {@class = "description"});
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
                {
                    return;
                }

                appService.CreateAsync(new CreateApplicationModel {Name = "Default Application"}).GetAwaiter()
                    .GetResult();
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

            ui.BeginDiv("card");
            {
                ui.BeginDiv("content");
                {
                    ui.Div("Create an Access Token", new {@class = "header"});
                    ui.Div(message, new {@class = "description"});
                }
                ui.EndDiv();

                ui.BeginDiv($"ui bottom attached button{buttonState}", id: id)
                    .Icon(icon, color).Literal("Create Token")
                    .EndDiv();
            }
            ui.EndDiv();

            if (ui.OnClick())
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

            ui.BeginDiv("card");
            {
                ui.BeginDiv("content");
                {
                    ui.Div("Configure Cluster Settings", new {@class = "header"});
                    ui.Div(message, new {@class = "description"});
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
                {
                    return;
                }

                var configSeed = ConfigurationLoader.FromEmbeddedJsonFile("seed.json");
                if (configSeed == null)
                {
                    return;
                }

                SqliteConfigurationHelper.MigrateToLatest("settings.db", configSeed);
                var configRoot = ui.Context.UiServices.GetRequiredService<IConfigurationRoot>();
                configRoot.Reload();

                var options = ui.Context.UiServices.GetRequiredService<ISaveOptions<SecurityOptions>>();
                if (AuthenticationExtensions.MaybeSelfCreateMissingKeys(options.Value.Tokens))
                {
                    options.TrySave("HQ:Security");
                }

                ui.Invalidate();
            }
        }
    }
}
