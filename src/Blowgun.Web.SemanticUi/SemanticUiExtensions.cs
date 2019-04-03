// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Blowdart.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Blowgun.Web.SemanticUi
{
    public static class SemanticUiExtensions
    {
        private static SemanticUiElements Elements(IServiceProvider ui) => ui.GetRequiredService<SemanticUiElements>();

        public static bool Button(this Ui ui, string text)
        {
            return Elements(ui).Button(ui, text);
        }
    }
}