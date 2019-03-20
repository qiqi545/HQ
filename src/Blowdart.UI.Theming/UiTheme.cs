// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Blowdart.UI.Theming
{
    public class UiTheme
    {
        public static UiTheme Default = new UiTheme();

        public ColorVariables ColorVariables { get; }
        public ImageVariables ImageVariables { get; }
        
        public UiTheme()
        {
            ColorVariables = new ColorVariables();
            ImageVariables = new ImageVariables();
        }
    }
}