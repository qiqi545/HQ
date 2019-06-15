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
using HQ.Platform.Node.UI.Models;
using HQ.Platform.Security.AspnetCore.Mvc.Models;
using Lime;
using Lime.Web;
using Lime.Web.SemanticUi;

namespace HQ.Platform.Node.UI.Pages
{
    public class SignIn : HtmlComponent<SignInModel>
    {
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

                    ui.BeginForm("ui large form", method: "POST");
                    {
                        ui.BeginDiv("ui stacked segment");
                        {
                            ui.BeginDiv("field");
                            {
                                switch (model.IdentityType)
                                {
                                    case IdentityType.Username:
                                        ui.BeginDiv("ui left icon input");
                                    {
                                        ui.Icon(InterfaceIcons.User);
                                        ui.Input(InputType.Text, new {name = "username", placeholder = "Username"});
                                    }
                                        ui.EndDiv();
                                        break;
                                    case IdentityType.Email:
                                        ui.BeginDiv("ui left icon input");
                                    {
                                        ui.Icon(InterfaceIcons.Envelope);
                                        ui.Input(InputType.Text, new {name = "email", placeholder = "Email Address"});
                                    }
                                        ui.EndDiv();
                                        break;
                                    case IdentityType.PhoneNumber:
                                        ui.BeginDiv("ui left icon input");
                                    {
                                        ui.Icon(FontAwesomeIcons.Phone);
                                        ui.Input(InputType.Text, new {name = "phone", placeholder = "Phone Number"});
                                    }
                                        ui.EndDiv();
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                            ui.EndDiv();

                            ui.BeginDiv("field");
                            {
                                ui.BeginDiv("ui left icon input");
                                {
                                    ui.Icon(FontAwesomeIcons.Lock);
                                    ui.Input(InputType.Password, new {name = "password", placeholder = "Password"});
                                }
                                ui.EndDiv();
                            }
                            ui.EndDiv();

                            var id = ui.NextId("signin").ToString();
                            ui.BeginButton("ui fluid large violet submit button", id: id)
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
