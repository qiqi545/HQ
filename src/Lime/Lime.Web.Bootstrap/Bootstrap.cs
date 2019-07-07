// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Lime.Web.Bootstrap
{
	public class Bootstrap : HtmlSystem
	{
		public override string StylesSection()
		{
			const string components = @"
<!-- Bootstrap -->
<link rel=""stylesheet"" type=""text/css"" href=""~/lib/bootstrap/dist/css/bootstrap.min.css"">";
			return components;
		}

		public override string ScriptsSection()
		{
			const string scripts = @"
<!-- Bootstrap -->
<script type=""text/javascript"" src=""~/lib/bootstrap/dist/js/bootstrap.min.js""></script>";
			return scripts;
		}
	}
}