// Copyright (c) Lime, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using UIKit;

namespace Lime.UI.iOS
{
    public class UiViewController : UIViewController
    {
        private readonly IServiceProvider _serviceProvider;

        public UiViewController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var layout = _serviceProvider.GetRequiredService<LayoutRoot>();
            var ui = Ui.CreateNew(layout.Services);
            ui.Begin();
            layout.Root(ui);
            ui.End();
        }

        public void HandleEvent(object sender, EventArgs args, Value128 id)
        {
            var layout = _serviceProvider.GetRequiredService<LayoutRoot>();
            var ui = Ui.CreateNew(layout.Services);
            ui.Begin();
            ui.Clicked.Add(id);
            layout.Root(ui);
            ui.End();
        }
    }
}
