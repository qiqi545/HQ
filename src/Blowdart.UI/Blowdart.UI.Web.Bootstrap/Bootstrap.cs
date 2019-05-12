// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Blowdart.UI.Web.Bootstrap
{
    public class Bootstrap : HtmlSystem
    {
        public override string StylesSection()
        {
            const string components = @"
<!-- Bootstrap -->
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/bootstrap/bootstrap-min.css"">";
            return components;
        }

        public override string ScriptsSection()
        {
	        const string scripts = @"
<!-- Bootstrap -->
<script type=""text/javascript"" src=""~/lib/bootstrap/dist/bootstrap.min.js""></script>";
            return scripts;
        }
    }
}