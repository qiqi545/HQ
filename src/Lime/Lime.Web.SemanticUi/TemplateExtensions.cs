using System.Collections.Generic;
using System.Linq;

namespace Lime.Web.SemanticUi
{
	public static class TemplateExtensions
	{
		public static Ui InlineDropdown(this Ui ui, string prefix, IEnumerable<IViewFragment> items, IViewFragment defaultItem = null, string postfix = null)
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
					ui.I(new { @class = "dropdown icon" });
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
