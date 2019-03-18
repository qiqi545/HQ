// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Blowdart.UI
{
    public abstract class UiComponent
    {
        public virtual string Name => null;
        public virtual void Render(Ui ui) { Render(ui, null); }
        public abstract void Render(Ui ui, dynamic model);
    }
}