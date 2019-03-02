// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Blowdart.UI
{
    public class NoSystem : UiSystem
    {
        public override void Begin()
        {
            Trace.WriteLine("Begin");
        }

        public override void End()
        {
            Trace.WriteLine("End");
        }

        public override bool Button(Ui ui, string text)
        {
            Trace.WriteLine($"Button: {text}");
            return false;
        }
    }
}