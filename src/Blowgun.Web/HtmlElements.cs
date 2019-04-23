﻿// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Blowdart.UI;
using Blowdart.UI.Web;
using Blowdart.UI.Web.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Blowgun.Web
{
    public class HtmlElements : Elements
    {
        private readonly HtmlSystem _html;

        public HtmlElements(IServiceProvider serviceProvider)
        {
            _html = (HtmlSystem)serviceProvider.GetRequiredService<UiSystem>();
        }

        public override bool Button(Ui ui, string text)
        {
            ui.NextId();
            return Clickable(ui, "button", text);
        }
		
        protected bool Clickable(Ui ui, string el, string text, object attr = null)
        {
            var id = ui.NextIdHash;
            _html.Dom.AppendTag(el, id, text);
            _html.Scripts.AppendEvent("click", id);
            return ui.Clicked.Contains(id);
        }
    }
}
