// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Blowdart.UI.Theming
{
    public class ImageVariables : Dictionary<NamedSizes, short>
    {
        public ImageVariables()
        {
            Add(NamedSizes.Mini, 35);
            Add(NamedSizes.Tiny, 80);
            Add(NamedSizes.Small, 150);
            Add(NamedSizes.Medium, 300);
            Add(NamedSizes.Large, 450);
            Add(NamedSizes.Big, 600);
            Add(NamedSizes.Huge, 800);
            Add(NamedSizes.Massive, 960);
        }
    }
}