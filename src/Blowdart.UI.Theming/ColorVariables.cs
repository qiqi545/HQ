// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Drawing;

namespace Blowdart.UI.Theming
{
    public class ColorVariables : Dictionary<NamedColors, Color>
    {
        public ColorVariables()
        {
            Add(NamedColors.Red, "#B03060".ToColor());
            Add(NamedColors.Orange, "#FE9A76".ToColor());
            Add(NamedColors.Yellow, "#FFD700".ToColor());
            Add(NamedColors.Olive, "#32CD32".ToColor());
            Add(NamedColors.Green, "#016936".ToColor());
            Add(NamedColors.Teal, "#008080".ToColor());
            Add(NamedColors.Blue, "#0E6EB8".ToColor());
            Add(NamedColors.Violet, "#EE82EE".ToColor());
            Add(NamedColors.Purple, "#B413EC".ToColor());
            Add(NamedColors.Pink, "#FF1493".ToColor());
            Add(NamedColors.Brown, "#A52A2A".ToColor());
            Add(NamedColors.Grey, "#A0A0A0".ToColor());
            Add(NamedColors.Black, "#000000".ToColor());

            Add(NamedColors.Primary, "#B03060".ToColor());
            Add(NamedColors.Secondary, "#B03060".ToColor());
        }
    }
}
