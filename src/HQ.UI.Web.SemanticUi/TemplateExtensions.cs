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

using System.Collections.Generic;
using System.Linq;

namespace HQ.UI.Web.SemanticUi
{
	public static class TemplateExtensions
	{
		public static Ui InlineDropdown(this Ui ui, string prefix, IEnumerable<IViewFragment> items,
			IViewFragment defaultItem = null, string postfix = null)
		{
			// ReSharper disable once PossibleMultipleEnumeration
			defaultItem = defaultItem ?? items.FirstOrDefault();
			if (defaultItem == null)
				return ui;

			ui.BeginSpan(null);
			{
				ui.Literal(prefix);
				ui.BeginDiv("ui inline dropdown");
				{
					ui.BeginDiv("text");
					{
						defaultItem.Render(ui);
					}
					ui.EndDiv();
					ui.I(new {@class = "dropdown icon"});
					ui.BeginDiv("menu");
					{
						// ReSharper disable once PossibleMultipleEnumeration
						foreach (var item in items)
						{
							ui.BeginDiv("item");
							item.Render(ui);
							ui.EndDiv();
						}
					}
					ui.EndDiv();
				}
				ui.EndDiv();
				ui.OnReady(@"
$('.ui.dropdown')
  .dropdown()
;");
			}
			ui.EndSpan();
			return ui;
		}
	}
}