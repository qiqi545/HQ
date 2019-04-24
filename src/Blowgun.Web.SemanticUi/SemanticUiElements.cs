// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Blowdart.UI;
using Blowdart.UI.Internal;
using Blowdart.UI.Web;

namespace Blowgun.Web.SemanticUi
{
    public class SemanticUiElements : HtmlElements
    {
        public SemanticUiElements(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override bool Button(Ui ui, string text)
        {
            ui.NextId();

            var @class = BuildString(sb =>
            { 
                sb.Append("ui button");
            });

            return Clickable(ui, "button", text, Attributes.Attr(new { @class }));
        }

        public static string BuildString(Action<StringBuilder> action)
        {
	        var sb = Pools.StringBuilderPool.Get();
	        try
	        {
		        action(sb);
		        return sb.ToString();
	        }
	        finally
	        {
		        Pools.StringBuilderPool.Return(sb);
	        }
        }
	}
}
