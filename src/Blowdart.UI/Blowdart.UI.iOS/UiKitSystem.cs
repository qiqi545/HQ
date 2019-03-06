// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CoreGraphics;
using UIKit;

namespace Blowdart.UI.iOS
{
    public class UiKitSystem : UiSystem
    {
        private readonly UiViewController _controller;

        public UiKitSystem(UiViewController controller)
        {
            _controller = controller;
        }

        public override void Begin()
        {
            foreach(var view in _controller.View.Subviews) {
                view.RemoveFromSuperview();
            }
        }

        public override void End() { }

        public override bool Button(Ui ui, string text)
        {
            ui.NextId();
            var id = ui.NextIdHash;

            var button = new UIButton(UIButtonType.System) { Frame = new CGRect(25, 25, 300, 150) };
            button.SetTitle(text, UIControlState.Normal);
            button.TouchUpInside += (sender, e) => { _controller.HandleEvent(sender, e, id); };

            _controller.Add(button);
            return ui.Clicked.Contains(id);
        }
    }
}