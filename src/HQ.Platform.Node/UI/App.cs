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
using System.Collections.Generic;
using System.Threading.Tasks;
using HQ.Integration.Sqlite.Sql;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Models;
using HQ.Platform.Node.UI.Models;
using HQ.Platform.Node.UI.Pages;
using HQ.Platform.Security.AspNetCore.Mvc.Models;
using HQ.Platform.Security.Configuration;
using HQ.UI;
using HQ.UI.Web;
using HQ.UI.Web.SemanticUi;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Node.UI
{
    [SemanticUi]
    [Title("HQ.io")]
    public class App
    {
        private readonly IApplicationService<IdentityApplication> _applicationService;
        private readonly IOptionsMonitor<IdentityOptionsExtended> _identityOptions;
        private readonly IOptionsMonitor<SecurityOptions> _securityOptions;

        private readonly ISignInService<IdentityUserExtended, IdentityTenant, IdentityApplication, string>
            _signInService;

        public App(
            ISignInService<IdentityUserExtended, IdentityTenant, IdentityApplication, string> signInService,
            IApplicationService<IdentityApplication> applicationService,
            IOptionsMonitor<IdentityOptionsExtended> identityOptions,
            IOptionsMonitor<SecurityOptions> securityOptions)
        {
            _signInService = signInService;
            _applicationService = applicationService;
            _identityOptions = identityOptions;
            _securityOptions = securityOptions;
        }

        [Handler("/")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public async Task Index(Ui ui)
        {
            await Dashboard(ui);
        }

        [Handler("setup")]
        public async Task SetUp(Ui ui)
        {
            await Dashboard(ui);
        }

        private async Task Dashboard(Ui ui)
        {
            var hasApplication = (await _applicationService.GetCountAsync()).Data > 0;
            var hasAccessToken = (_securityOptions.CurrentValue.SuperUser?.Enabled).GetValueOrDefault();
            var hasSettings = !SqliteConfigurationHelper.IsEmptyConfiguration("settings.db");

            ui.Component<Dashboard>(new DashboardModel
            {
                HasApplication = hasApplication, HasAccessToken = hasAccessToken, HasSettings = hasSettings
            });
        }

        [Handler("signin")]
        public async Task SignIn(Ui ui, SignInForm form)
        {
            if (ui.Context.User.Identity.IsAuthenticated)
            {
                // TODO redirect to dashboard
                return;
            }

            form.identityType = form.identityType ??
                                _identityOptions.CurrentValue.DefaultIdentityType.GetValueOrDefault(IdentityType.Email);

            string identity;
            switch (form.identityType)
            {
                case IdentityType.Username:
                    identity = form.username;
                    break;
                case IdentityType.Email:
                    identity = form.email;
                    break;
                case IdentityType.PhoneNumber:
                    identity = form.phoneNumber;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(form.identityType), form.identityType, null);
            }

            if (form.email != null && form.password != null)
            {
                var operation = await _signInService.SignInAsync(form.identityType.Value, identity, form.password,
                    form.rememberMe);
                if (operation.Succeeded)
                {
                    return;
                }
            }

            ui.Component<SignIn>(new SignInModel
            {
                IdentityType = form.identityType, Identity = identity, Password = form.password
            });
        }

        [Handler("signout")]
        public void SignOut(Ui ui)
        {
            ui.Literal("Sign Out");
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
