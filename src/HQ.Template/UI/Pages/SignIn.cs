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

using HQ.Platform.Identity.Models;
using Lime;
using Lime.Web;
using Lime.Web.SemanticUi;
using Microsoft.AspNetCore.Identity;
using HQ.Template.UI.Models;

namespace HQ.Template.UI.Pages
{
    public class SignIn : HtmlComponent<SignInModel>
    {
        private readonly ISignInService<IdentityUser> _signInService;

        public SignIn(ISignInService<IdentityUser> signInService)
        {
            _signInService = signInService;
        }

        public override void Render(Ui ui, SignInModel model)
        {
            ui.Styles(@"
    body {
      background-color: #DADADA;
    }
    body > .grid {
      height: 100%;
    }
    .image {
      margin-top: -100px;
    }
    .column {
      max-width: 450px;
    }
    #ui-body {
      margin-top: 300px;
    }
");
            
            ui.BeginDiv("ui middle aligned center aligned grid");
            {
                ui.BeginDiv("column");
                {
                    ui.BeginH2("ui violet image header");
                    {
                        ui.Img(new {src = "assets/images/logo.png", @class = "image"});
                    }
                    ui.EndH2();

                    ui.BeginForm("ui large form");
                    {
                        ui.BeginDiv("ui stacked segment");
                        {
                            ui.BeginDiv("field");
                            {
                                ui.BeginDiv("ui left icon input");
                                {
                                    ui.Icon(InterfaceIcons.User);
                                    ui.Input(InputType.Text, new { name = "email", placeholder = "Email Address" });
                                }
                                ui.EndDiv();
                            }
                            ui.EndDiv();

                            ui.BeginDiv("field");
                            {
                                ui.BeginDiv("ui left icon input");
                                {
                                    ui.Icon(FontAwesomeIcons.Lock);
                                    ui.Input(InputType.Password, new { name = "password", placeholder = "Password" });
                                }
                                ui.EndDiv();
                            }
                            ui.EndDiv();

                            var id = ui.NextId("signin").ToString();
                            ui.BeginButton(@class: "ui fluid large violet submit button", id: id)
                                .Literal("Sign In")
                                .EndInput();
                            if (ui.OnClick())
                            {
                                ui.Invalidate();
                            }
                        }
                        ui.EndDiv();

                        ui.Div(new {@class = "ui error message"});
                    }
                    ui.EndForm();
                }
                ui.EndDiv();
            }
            ui.EndDiv();
        }
    }
}
