﻿// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Blowdart.UI.Web
{
    public abstract class HtmlComponent : UiComponent
    {
        public static Attributes Attr(object attr)
        {
            return Attributes.Attr(attr);
        }
    }
}