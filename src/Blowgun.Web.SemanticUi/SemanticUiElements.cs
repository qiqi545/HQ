// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Blowdart.UI;
using Blowdart.UI.Web.Internal;

namespace Blowgun.Web.SemanticUi
{
    public class SemanticUiElements : HtmlElements
    {
        public SemanticUiElements(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override bool Button(Ui ui, string text)
        {
            ui.NextId();

            var @class = StringBuilderHelper.BuildString(sb =>
            {
                sb.Append("ui button");
            });

            return Clickable(ui, "button", text, Attr(new { @class }));
        }
    }
}
