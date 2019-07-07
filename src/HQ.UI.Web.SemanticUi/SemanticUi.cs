#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

namespace HQ.UI.Web.SemanticUi
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
<script type=""text/javascript"" src=""~/lib/semantic-ui/components/dropdown.min.js""></script>
<script type=""text/javascript"" src=""~/lib/semantic-ui-log.js""></script>";
			return scripts;
		}
	}
}