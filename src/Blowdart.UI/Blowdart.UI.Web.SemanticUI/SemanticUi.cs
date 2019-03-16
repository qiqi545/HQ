// Copyright (c) Blowdart, Inc. All rights reserved.
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
            Dom.AppendTag("button", id, text, Attributes.Attr(new {@class = attr}));
            Scripts.AppendClick(id);
            return ui.Clicked.Contains(id);
        }

        public override string StylesSection()
        {
            const string components = @"
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/semantic-ui/components/reset.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/semantic-ui/components/site.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/semantic-ui/components/container.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/semantic-ui/components/grid.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/semantic-ui/components/header.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/semantic-ui/components/image.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/semantic-ui/components/menu.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/semantic-ui/components/divider.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/semantic-ui/components/list.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/semantic-ui/components/segment.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/semantic-ui/components/dropdown.css"">
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/semantic-ui/components/icon.css"">
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

            const string logging = @"<script type=""text/javascript"">
document.addEventListener(""DOMContentLoaded"", function(event) {
    var clear = $('#clear-logs');
    if(clear) {
        clear.click(function() {
            $('#no-logs').addClass('active');
            $('#logs').empty();
        });
    }
});

logMessage = function(m) {
    var lvl;
    switch (m.level) {
        case 0:
            lvl = ""Trace"";
            break;
        case 1:
            lvl = ""Debug"";
            break;
        case 2:
            lvl = ""Information"";
            break;
        case 3:
            lvl = ""Warning"";
            break;
        case 4:
            lvl = ""Error"";
            break;
        case 5:
            lvl = ""Critical"";
            break;
        default:
            lvl = ""Unknown"";
            break;
    }
    var div =
        ""<div class='item'>"" +
            ""<i class='large envelope middle aligned icon'></i>"" +
            ""<div class='content'>"" +
                ""<a class='header'>"" + lvl + ""</a>"" +
                ""<div class='description'>"" + m.message + ""</div>"" +
            ""</div>"" +
        ""</div>"";
    var feed = $(""#logs"");
    if (feed !== null) {
        feed.append(div);
        $(""#no-logs"").removeClass(""active"");
    }
}
</script>";

            return scripts + logging;
        }
    }
}