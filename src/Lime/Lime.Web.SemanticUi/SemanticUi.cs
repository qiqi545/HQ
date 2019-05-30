// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Lime.Web.SemanticUi
{
	public class SemanticUi : HtmlSystem
	{
		public override string StylesSection()
		{
			const string components = @"
    <!-- Semantic-UI -->
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
    <link rel=""stylesheet"" type=""text/css"" href=""~/lib/semantic-ui/semantic.min.css"">";
			return components;
		}

		public override string ScriptsSection()
		{
			const string scripts = @"
<!-- Semantic-UI -->
<script type=""text/javascript"" src=""~/lib/jquery/dist/jquery.slim.min.js""></script>
<script type=""text/javascript"" src=""~/lib/semantic-ui/semantic.min.js""></script>
<script type=""text/javascript"" src=""~/lib/semantic-ui-log.js""></script>";
			return scripts;
		}
	}
}