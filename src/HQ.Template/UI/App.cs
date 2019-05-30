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

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using HQ.Data.Sql.Sqlite;
using HQ.Platform.Identity.Models;
using HQ.Platform.Security.Configuration;
using Lime;
using Lime.Web.SemanticUi;
using HQ.Template.UI.Models;
using HQ.Template.UI.Pages;

namespace HQ.Template.UI
{
    [SemanticUi, Title("HQ.io")]
    public class App
    {
        private readonly IApplicationService<IdentityApplication> _applicationService;
        private readonly IOptionsMonitor<SecurityOptions> _securityOptions;

        public App(IApplicationService<IdentityApplication> applicationService, IOptionsMonitor<SecurityOptions> securityOptions)
        {
            _applicationService = applicationService;
            _securityOptions = securityOptions;
        }

        [Handler("/")]
        public async Task Index(Ui ui, string email, string password)
        {
            var hasApplication = (await _applicationService.GetCountAsync()).Data > 0;
            var hasAccessToken = (_securityOptions.CurrentValue.SuperUser?.Enabled).GetValueOrDefault();
            var hasSettings = !SqliteConfigurationHelper.IsEmptyConfiguration("settings.db");

            var model = new DashboardModel
            {
                HasApplication = hasApplication,
                HasAccessToken = hasAccessToken,
                HasSettings = hasSettings
            };

            if (model.IsFirstTimeExperience || ui.Context.User.Identity.IsAuthenticated)
            {
                ui.Component<Dashboard>(model);
            }
            else
            {
                ui.Component<SignIn>(new SignInModel { Email = email, Password = password });
            }
        }

        [Handler("apps")]
        public void ManageApplications(Ui ui)
        {
            ui.Component<ManageApplications>(new List<IdentityApplication>());
        }

        [Handler("logstream")]
        public void LogStream(Ui ui)
        {
            ui.Component<LogStream>();
        }
    }
}
