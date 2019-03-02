﻿// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Blowdart.UI.Web.Internal;

namespace Blowdart.UI.Web.SemanticUI
{
    public class SemanticUi : HtmlSystem
    {
        public override bool Button(Ui ui, string text)
        {
            ui.NextId();

            var attr = BuildString(sb => { sb.Append("ui button"); });

            var id = ui.NextIdHash;
            Dom.AppendTag("button", id, text, new {@class = attr});
            Scripts.AppendClick(id);
            return ui.Clicked.Contains(id);
        }

        public override string StylesSection()
        {
            const string components = @"
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/components/reset.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/components/site.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/components/container.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/components/grid.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/components/header.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/components/image.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/components/menu.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/components/divider.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/components/list.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/components/segment.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/components/dropdown.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/components/icon.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/semantic-ui/semantic.min.css"">
";
            return components;
        }

        public override string ScriptsSection()
        {
            const string scripts = @"
<script type=""text/javascript"" src=""~/lib/jquery/dist/jquery.slim.min.js""></script>
<script type=""text/javascript"" src=""~/lib/semantic-ui/semantic.min.js""></script>
";
            return scripts;
        }
    }
}